using Common.Contracts.authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Module.Inventory.Application.UseCases.Transfers.Resolve;
using Module.Inventory.Domain.Inventory;
using Module.Inventory.Domain.Products;
using Module.Inventory.Domain.Transfers;
using System.Infrastructure.Persistence;

namespace Test.Inventory;

public class ResolveStockTransferTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid FromBranchId = Guid.NewGuid();
    private static readonly Guid ToBranchId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid VariantId = Guid.NewGuid();
    private static readonly Guid TransferId = Guid.NewGuid();

    private static TestAppDbContext CreateDbContext()
    {
        var tenantCtx = new TestTenantConnectionContext
        {
            TenantId = TenantId,
            Schema = "test_schema",
            DatabaseName = "test_db"
        };
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"InvTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new TestAppDbContext(options, tenantCtx);
    }

    private static ActorContext CreateActorContext(Guid branch)
        => new(TenantId, UserId, "Test User", branch, [branch]);

    private static async Task SeedVariant(TestAppDbContext ctx, int fromStock, int toStock)
    {
        var brand = new Brand { Id = Guid.NewGuid(), Name = "Nike", Prefix = "NIK", CreatedBy = TenantId, CreatedByName = "Test User" };
        var category = new Category { Id = Guid.NewGuid(), Name = "Zapatillas", CreatedBy = TenantId, CreatedByName = "Test User" };
        var color = new Color { Id = Guid.NewGuid(), Name = "Negro", CreatedBy = TenantId, CreatedByName = "Test User" };
        var size = new Size { Id = Guid.NewGuid(), Name = "42", SortOrder = 1, CreatedBy = TenantId, CreatedByName = "Test User" };
        ctx.Brands.Add(brand);
        ctx.Categories.Add(category);
        ctx.Colors.Add(color);
        ctx.Sizes.Add(size);

        var product = Product.Create("Air Max", "d", category.Id, brand.Id, Gender.Unisex, "NIK-1", TenantId, UserId, "Test User");
        ctx.Products.Add(product);
        await ctx.SaveChangesAsync();

        var variant = ProductVariant.Create(product.Id, color.Id, size.Id, 100m, "NIK-1-001", TenantId, UserId, "Test User");
        variant.Id = VariantId;
        ctx.ProductVariants.Add(variant);
        await ctx.SaveChangesAsync();

        ctx.BranchInventories.Add(new BranchInventory
        {
            ProductVariantId = VariantId,
            BranchId = FromBranchId,
            Stock = fromStock,
            CreatedBy = UserId,
            CreatedByName = "Test User"
        });
        ctx.BranchInventories.Add(new BranchInventory
        {
            ProductVariantId = VariantId,
            BranchId = ToBranchId,
            Stock = toStock,
            CreatedBy = UserId,
            CreatedByName = "Test User"
        });
        await ctx.SaveChangesAsync();
    }

    private static async Task SeedPendingTransfer(TestAppDbContext ctx, int quantity, int fromStock, int toStock)
    {
        await SeedVariant(ctx, fromStock, toStock);

        var transfer = new StockTransfer
        {
            Id = TransferId,
            FromBranchId = FromBranchId,
            ToBranchId = ToBranchId,
            RequestedByUserId = UserId,
            Status = TransferStatus.Pending,
            CreatedBy = UserId,
            CreatedByName = "Test User"
        };
        transfer.Items.Add(new StockTransferItem
        {
            ProductVariantId = VariantId,
            QuantityRequested = quantity,
            CreatedBy = UserId,
            CreatedByName = "Test User"
        });
        ctx.StockTransfers.Add(transfer);
        await ctx.SaveChangesAsync();
    }

    private static ResolveStockTransfer CreateSut(TestAppDbContext ctx)
        => new(ctx);

    [Fact]
    public async Task Execute_ShouldMoveStock_AndCreateMovements()
    {
        using var ctx = CreateDbContext();
        await SeedPendingTransfer(ctx, 3, 10, 0);
        var sut = CreateSut(ctx);

        var result = await sut.Execute(CreateActorContext(ToBranchId), TransferId, new ResolveStockTransferDto { Complete = true });

        Assert.True(result.IsSuccess, $"Expected success but got: {result.Error?.Code} - {result.Error?.Message}");

        var transfer = await ctx.StockTransfers.Include(t => t.Items).SingleAsync();
        Assert.Equal(TransferStatus.Completed, transfer.Status);

        var fromInv = await ctx.BranchInventories.SingleAsync(bi => bi.ProductVariantId == VariantId && bi.BranchId == FromBranchId);
        var toInv = await ctx.BranchInventories.SingleAsync(bi => bi.ProductVariantId == VariantId && bi.BranchId == ToBranchId);
        Assert.Equal(7, fromInv.Stock);
        Assert.Equal(3, toInv.Stock);

        var movements = await ctx.StockMovements.Where(m => m.ReferenceId == TransferId).ToListAsync();
        Assert.Equal(2, movements.Count);
        Assert.Contains(movements, m => m.MovementType == MovementType.TransferOut && m.Quantity == -3m && m.BranchId == FromBranchId);
        Assert.Contains(movements, m => m.MovementType == MovementType.TransferIn && m.Quantity == 3m && m.BranchId == ToBranchId);
    }

    [Fact]
    public async Task Execute_ShouldReject_WithoutMovingStock()
    {
        using var ctx = CreateDbContext();
        await SeedPendingTransfer(ctx, 3, 10, 0);
        var sut = CreateSut(ctx);

        var result = await sut.Execute(CreateActorContext(ToBranchId), TransferId, new ResolveStockTransferDto { Complete = false, Notes = "Rechazado" });

        Assert.True(result.IsSuccess);
        Assert.Equal(TransferStatus.Rejected, (await ctx.StockTransfers.SingleAsync()).Status);

        var fromInv = await ctx.BranchInventories.SingleAsync(bi => bi.ProductVariantId == VariantId && bi.BranchId == FromBranchId);
        Assert.Equal(10, fromInv.Stock);
        Assert.Empty(await ctx.StockMovements.ToListAsync());
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenInsufficientStockOnResolve()
    {
        using var ctx = CreateDbContext();
        await SeedPendingTransfer(ctx, 5, 2, 0);
        var sut = CreateSut(ctx);

        var result = await sut.Execute(CreateActorContext(ToBranchId), TransferId, new ResolveStockTransferDto { Complete = true });

        Assert.False(result.IsSuccess);
        Assert.Equal(ResolveStockTransferErrors.InsufficientStock, result.Error);

        var transfer = await ctx.StockTransfers.SingleAsync();
        Assert.Equal(TransferStatus.Pending, transfer.Status);
        Assert.Equal(2, (await ctx.BranchInventories.SingleAsync(bi => bi.ProductVariantId == VariantId && bi.BranchId == FromBranchId)).Stock);
        Assert.Empty(await ctx.StockMovements.ToListAsync());
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenWrongBranch()
    {
        using var ctx = CreateDbContext();
        await SeedPendingTransfer(ctx, 1, 10, 0);
        var sut = CreateSut(ctx);

        var result = await sut.Execute(CreateActorContext(FromBranchId), TransferId, new ResolveStockTransferDto { Complete = true });

        Assert.False(result.IsSuccess);
        Assert.Equal(ResolveStockTransferErrors.Forbidden, result.Error);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenAlreadyResolved()
    {
        using var ctx = CreateDbContext();
        await SeedPendingTransfer(ctx, 1, 10, 0);
        var transfer = await ctx.StockTransfers.SingleAsync();
        transfer.Status = TransferStatus.Completed;
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);
        var result = await sut.Execute(CreateActorContext(ToBranchId), TransferId, new ResolveStockTransferDto { Complete = true });

        Assert.False(result.IsSuccess);
        Assert.Equal(ResolveStockTransferErrors.AlreadyResolved, result.Error);
    }
}