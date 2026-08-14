using Common.Contracts.authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Module.Inventory.Application.UseCases.ProductVariants.Delete;
using Module.Inventory.Domain.Inventory;
using Module.Inventory.Domain.Products;
using Module.Inventory.Domain.Transfers;
using System.Infrastructure.Persistence;

namespace Test.Inventory;

public class DeleteProductVariantTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid BranchId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid VariantId = Guid.NewGuid();

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

    private static ICurrentUser CreateCurrentUser()
    {
        var mock = new Mock<ICurrentUser>();
        mock.Setup(u => u.UserId).Returns(UserId);
        mock.Setup(u => u.FullName).Returns("Test User");
        return mock.Object;
    }

    private static async Task SeedVariant(TestAppDbContext ctx, bool withMovement)
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

        if (withMovement)
        {
            ctx.StockMovements.Add(StockMovement.CreateReception(
                BranchId, VariantId, UserId, "Test User", 5, Guid.NewGuid()));
            await ctx.SaveChangesAsync();
        }
    }

    private static async Task SeedVariantInTransfer(TestAppDbContext ctx)
    {
        await SeedVariant(ctx, withMovement: false);

        var transfer = new StockTransfer
        {
            FromBranchId = Guid.NewGuid(),
            ToBranchId = Guid.NewGuid(),
            RequestedByUserId = UserId,
            Status = TransferStatus.Pending,
            CreatedBy = UserId,
            CreatedByName = "Test User"
        };
        transfer.Items.Add(new StockTransferItem
        {
            ProductVariantId = VariantId,
            QuantityRequested = 1,
            CreatedBy = UserId,
            CreatedByName = "Test User"
        });
        ctx.StockTransfers.Add(transfer);
        await ctx.SaveChangesAsync();
    }

    private static DeleteProductVariantUc CreateSut(TestAppDbContext ctx)
        => new(ctx, CreateCurrentUser());

    [Fact]
    public async Task Check_ShouldReturnCanDelete_WhenNoMovements()
    {
        using var ctx = CreateDbContext();
        await SeedVariant(ctx, withMovement: false);
        var sut = CreateSut(ctx);

        var result = await sut.Check(VariantId);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.CanDelete);
        Assert.Equal(string.Empty, result.Value.Reason);
    }

    [Fact]
    public async Task Check_ShouldReturnHasMovements_WhenVariantHasMovements()
    {
        using var ctx = CreateDbContext();
        await SeedVariant(ctx, withMovement: true);
        var sut = CreateSut(ctx);

        var result = await sut.Check(VariantId);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.CanDelete);
        Assert.Equal("HAS_MOVEMENTS", result.Value.Reason);
    }

    [Fact]
    public async Task Check_ShouldReturnNotFound_WhenVariantMissing()
    {
        using var ctx = CreateDbContext();
        var sut = CreateSut(ctx);

        var result = await sut.Check(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(DeleteProductVariantErrors.VariantNotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ShouldSoftDelete_WhenNoMovements()
    {
        using var ctx = CreateDbContext();
        await SeedVariant(ctx, withMovement: false);
        var sut = CreateSut(ctx);

        var result = await sut.Execute(VariantId);

        Assert.True(result.IsSuccess);

        var variant = await ctx.ProductVariants
            .IgnoreQueryFilters()
            .SingleAsync(v => v.Id == VariantId);
        Assert.NotNull(variant.DeletedAt);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenVariantHasMovements()
    {
        using var ctx = CreateDbContext();
        await SeedVariant(ctx, withMovement: true);
        var sut = CreateSut(ctx);

        var result = await sut.Execute(VariantId);

        Assert.False(result.IsSuccess);
        Assert.Equal(DeleteProductVariantErrors.VariantHasMovements, result.Error);

        var variant = await ctx.ProductVariants.IgnoreQueryFilters().SingleAsync(v => v.Id == VariantId);
        Assert.Null(variant.DeletedAt);
    }

    [Fact]
    public async Task Check_ShouldReturnHasTransfer_WhenReferencedInTransfer()
    {
        using var ctx = CreateDbContext();
        await SeedVariantInTransfer(ctx);
        var sut = CreateSut(ctx);

        var result = await sut.Check(VariantId);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.CanDelete);
        Assert.Equal("HAS_TRANSFER", result.Value.Reason);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenReferencedInTransfer()
    {
        using var ctx = CreateDbContext();
        await SeedVariantInTransfer(ctx);
        var sut = CreateSut(ctx);

        var result = await sut.Execute(VariantId);

        Assert.False(result.IsSuccess);
        Assert.Equal(DeleteProductVariantErrors.VariantHasTransfers, result.Error);

        var variant = await ctx.ProductVariants.IgnoreQueryFilters().SingleAsync(v => v.Id == VariantId);
        Assert.Null(variant.DeletedAt);
    }
}