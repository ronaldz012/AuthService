using Common.Contracts.authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Module.Inventory.Application.UseCases.Receptions.Revert;
using Module.Inventory.Domain.Inventory;
using Module.Inventory.Domain.Products;
using Module.Inventory.Domain.Receptions;
using System.Infrastructure.Persistence;

namespace Test.Inventory;

public class RevertStockReceptionTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid BranchId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ProviderId = Guid.NewGuid();
    private static readonly Guid SizeId = Guid.NewGuid();

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

    private static ActorContext CreateActorContext()
        => new(TenantId, UserId, "Test User", BranchId, [BranchId]);

    private static RevertStockReception CreateSut(TestAppDbContext ctx)
        => new(ctx, NullLogger<RevertStockReception>.Instance);

    private static ProductVariant SeedVariant(TestAppDbContext ctx, int stock)
    {
        ctx.Sizes.Add(new Size { Id = SizeId, Name = "L", SortOrder = 1, CreatedBy = UserId, CreatedByName = "Test User" });
        ctx.SaveChangesAsync().GetAwaiter().GetResult();

        var variant = ProductVariant.Create(
            Guid.NewGuid(), Guid.NewGuid(), SizeId, 100m,
            $"SKU-{Guid.NewGuid():N}", TenantId, UserId, "Test User");
        ctx.ProductVariants.Add(variant);
        ctx.SaveChangesAsync().GetAwaiter().GetResult();

        ctx.BranchInventories.Add(new BranchInventory
        {
            ProductVariantId = variant.Id,
            BranchId = BranchId,
            Stock = stock,
            CreatedBy = UserId,
            CreatedByName = "Test User"
        });
        ctx.SaveChangesAsync().GetAwaiter().GetResult();

        return variant;
    }

    private static StockReception SeedReception(
        TestAppDbContext ctx,
        ProductVariant variant,
        int quantity,
        DateTime? receivedAt = null,
        ReceptionStatus status = ReceptionStatus.Confirmed)
    {
        var reception = StockReception.Create(BranchId, UserId, "Test User", null, ProviderId);
        reception.ReceivedAt = receivedAt ?? DateTime.UtcNow;
        reception.Status = status;
        reception.AddExistingVariant(variant.Id, UserId, "Test User", quantity, 50m);

        ctx.StockReceptions.Add(reception);
        ctx.SaveChangesAsync().GetAwaiter().GetResult();

        return reception;
    }

    [Fact]
    public async Task Check_ShouldReturnCanRevert_WhenEligible()
    {
        using var ctx = CreateDbContext();
        var variant = SeedVariant(ctx, 100);
        var reception = SeedReception(ctx, variant, 10);
        var sut = CreateSut(ctx);

        var result = await sut.Check(CreateActorContext(), reception.Id);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.CanRevert);
        Assert.Equal(string.Empty, result.Value.Reason);
    }

    [Fact]
    public async Task Check_ShouldReturnNotEnoughStock_WhenBranchStockIsInsufficient()
    {
        using var ctx = CreateDbContext();
        var variant = SeedVariant(ctx, 5);
        var reception = SeedReception(ctx, variant, 10);
        var sut = CreateSut(ctx);

        var result = await sut.Check(CreateActorContext(), reception.Id);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.CanRevert);
        Assert.Equal("NOT_ENOUGH_STOCK", result.Value.Reason);
    }

    [Fact]
    public async Task Check_ShouldReturnOutdated_WhenReceptionIsOld()
    {
        using var ctx = CreateDbContext();
        var variant = SeedVariant(ctx, 100);
        var reception = SeedReception(ctx, variant, 10, receivedAt: DateTime.UtcNow.AddDays(-2));
        var sut = CreateSut(ctx);

        var result = await sut.Check(CreateActorContext(), reception.Id);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.CanRevert);
        Assert.Equal("OUTDATED", result.Value.Reason);
    }

    [Fact]
    public async Task Check_ShouldReturnAlreadyReverted_WhenStatusIsReverted()
    {
        using var ctx = CreateDbContext();
        var variant = SeedVariant(ctx, 100);
        var reception = SeedReception(ctx, variant, 10, status: ReceptionStatus.Reverted);
        var sut = CreateSut(ctx);

        var result = await sut.Check(CreateActorContext(), reception.Id);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.CanRevert);
        Assert.Equal("ALREADY_REVERTED", result.Value.Reason);
    }

    [Fact]
    public async Task Check_ShouldReturnNotFound_WhenReceptionDoesNotExist()
    {
        using var ctx = CreateDbContext();
        var sut = CreateSut(ctx);

        var result = await sut.Check(CreateActorContext(), Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(RevertStockReceptionErrors.ReceptionNotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ShouldRevertReception_AndDeductStock()
    {
        using var ctx = CreateDbContext();
        var variant = SeedVariant(ctx, 100);
        var reception = SeedReception(ctx, variant, 10);
        var sut = CreateSut(ctx);

        var result = await sut.Execute(CreateActorContext(), reception.Id);

        Assert.True(result.IsSuccess);

        var updated = await ctx.StockReceptions.FindAsync(reception.Id);
        Assert.Equal(ReceptionStatus.Reverted, updated!.Status);

        var inventory = await ctx.BranchInventories
            .SingleAsync(bi => bi.ProductVariantId == variant.Id && bi.BranchId == BranchId);
        Assert.Equal(90, inventory.Stock);

        var movement = await ctx.StockMovements
            .SingleAsync(m => m.ReferenceId == reception.Id);
        Assert.Equal(MovementType.ReceptionRevert, movement.MovementType);
        Assert.Equal(-10m, movement.Quantity);
    }

    [Fact]
    public async Task Execute_ShouldRevertAverageCost_UsingOriginalUnitCost()
    {
        using var ctx = CreateDbContext();
        var variant = SeedVariant(ctx, 20);
        variant.AverageCost = 20m;
        await ctx.SaveChangesAsync();

        // Recepción de 10u @10 (original), con stock total 20 @ promedio 20
        var reception = StockReception.Create(BranchId, UserId, "Test User", null, ProviderId);
        reception.AddExistingVariant(variant.Id, UserId, "Test User", 10, 10m);
        ctx.StockReceptions.Add(reception);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);
        var result = await sut.Execute(CreateActorContext(), reception.Id);

        Assert.True(result.IsSuccess);

        var updated = await ctx.ProductVariants.SingleAsync(pv => pv.Id == variant.Id);
        // nuevo_stock = 20 - 10 = 10
        // nuevo_promedio = (20*20 - 10*10) / 10 = 30
        Assert.Equal(10, (await ctx.BranchInventories.SingleAsync(bi => bi.ProductVariantId == variant.Id)).Stock);
        Assert.Equal(30m, updated.AverageCost);
    }

    [Fact]
    public async Task Check_ShouldBlock_WhenVariantHasSaleAfterReception()
    {
        using var ctx = CreateDbContext();
        var variant = SeedVariant(ctx, 100);
        var reception = SeedReception(ctx, variant, 10);
        var sut = CreateSut(ctx);

        ctx.StockMovements.Add(StockMovement.CreateSale(
            BranchId, variant.Id, UserId, "Test User", 1, Guid.NewGuid(), 50m, null));
        await ctx.SaveChangesAsync();

        var result = await sut.Check(CreateActorContext(), reception.Id);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.CanRevert);
        Assert.Equal("CONTAMINATED_BY_SALES_OR_ADJUSTMENTS", result.Value.Reason);
    }

    [Fact]
    public async Task Check_ShouldBlock_WhenVariantHasAdjustmentAfterReception()
    {
        using var ctx = CreateDbContext();
        var variant = SeedVariant(ctx, 100);
        var reception = SeedReception(ctx, variant, 10);
        var sut = CreateSut(ctx);

        ctx.StockMovements.Add(StockMovement.CreateAdjustment(
            BranchId, variant.Id, UserId, "Test User", -1, "Faltante", 50m));
        await ctx.SaveChangesAsync();

        var result = await sut.Check(CreateActorContext(), reception.Id);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.CanRevert);
        Assert.Equal("CONTAMINATED_BY_SALES_OR_ADJUSTMENTS", result.Value.Reason);
    }

    [Fact]
    public async Task Execute_ShouldReturnContaminationError_WithoutPersistingChanges()
    {
        using var ctx = CreateDbContext();
        var variant = SeedVariant(ctx, 100);
        var reception = SeedReception(ctx, variant, 10);
        var sut = CreateSut(ctx);

        ctx.StockMovements.Add(StockMovement.CreateAdjustment(
            BranchId, variant.Id, UserId, "Test User", -1, "Faltante", 50m));
        await ctx.SaveChangesAsync();

        var result = await sut.Execute(CreateActorContext(), reception.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(RevertStockReceptionErrors.ContaminatedBySalesOrAdjustments, result.Error);

        var updated = await ctx.StockReceptions.FindAsync(reception.Id);
        Assert.Equal(ReceptionStatus.Confirmed, updated!.Status);
    }

    [Fact]
    public async Task Execute_ShouldReturnNotEnoughStock_WithoutPersistingChanges()
    {
        using var ctx = CreateDbContext();
        var variant = SeedVariant(ctx, 5);
        var reception = SeedReception(ctx, variant, 10);
        var sut = CreateSut(ctx);

        var result = await sut.Execute(CreateActorContext(), reception.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(RevertStockReceptionErrors.NotEnoughStock, result.Error);

        var updated = await ctx.StockReceptions.FindAsync(reception.Id);
        Assert.Equal(ReceptionStatus.Confirmed, updated!.Status);

        var inventory = await ctx.BranchInventories
            .SingleAsync(bi => bi.ProductVariantId == variant.Id && bi.BranchId == BranchId);
        Assert.Equal(5, inventory.Stock);

        Assert.Empty(await ctx.StockMovements.Where(m => m.ReferenceId == reception.Id).ToListAsync());
    }
}