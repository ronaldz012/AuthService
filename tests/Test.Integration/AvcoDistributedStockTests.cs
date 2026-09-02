using Microsoft.EntityFrameworkCore;
using Module.Inventory.Domain.Inventory;
using Module.Inventory.Domain.Products;
using Module.Inventory.Infrastructure;
using System.Infrastructure.Persistence;

namespace Test.Integration;

public class AvcoDistributedStockTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid BranchA = Guid.NewGuid();
    private static readonly Guid BranchB = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid VariantId = Guid.NewGuid();

    private static AppDbContext CreateDbContext()
    {
        var tenantCtx = TestIntegrationDbContextFactory.CreateTenantContext(TenantId);
        return TestIntegrationDbContextFactory.Create(tenantCtx);
    }

    private static async Task SeedVariantDistributed(AppDbContext ctx, decimal avgCost, int stockA, int stockB)
    {
        var brand = new Brand { Id = Guid.NewGuid(), Name = "Nike", Prefix = "NIK", TenantId = TenantId, CreatedBy = TenantId, CreatedByName = "Test" };
        var category = new Category { Id = Guid.NewGuid(), Name = "Zapatillas", TenantId = TenantId, CreatedBy = TenantId, CreatedByName = "Test" };
        var color = new Color { Id = Guid.NewGuid(), Name = "Negro", TenantId = TenantId };
        var size = new Size { Id = Guid.NewGuid(), Name = "42", SortOrder = 1, TenantId = TenantId };
        ctx.Brands.Add(brand);
        ctx.Categories.Add(category);
        ctx.Colors.Add(color);
        ctx.Sizes.Add(size);
        var product = Product.Create("Air Max", "d", category.Id, brand.Id, Gender.Unisex, "ADI-1", TenantId, UserId, "Test User");
        ctx.Products.Add(product);
        await ctx.SaveChangesAsync();

        var variant = ProductVariant.Create(product.Id, color.Id, size.Id, 75m, "ADI1-001", TenantId, UserId, "Test User");
        variant.Id = VariantId;
        variant.AverageCost = avgCost;
        ctx.ProductVariants.Add(variant);
        await ctx.SaveChangesAsync();

        ctx.BranchInventories.Add(new BranchInventory { ProductVariantId = VariantId, BranchId = BranchA, TenantId = TenantId, Stock = stockA, CreatedBy = UserId, CreatedByName = "Test User" });
        ctx.BranchInventories.Add(new BranchInventory { ProductVariantId = VariantId, BranchId = BranchB, TenantId = TenantId, Stock = stockB, CreatedBy = UserId, CreatedByName = "Test User" });
        await ctx.SaveChangesAsync();
    }

    [Fact]
    public async Task Return_InBranchA_ShouldUseGlobalStock_ForAverageCost()
    {
        // Repro del bug reportado: stock distribuido A+B, venta/recepción/devolución solo en A
        // Inicial: ADI1-001 75 Bs, total 10 (A 5 + B 5) => avg 75
        // 1 venta en A 1 ud @75 => A 4, B 5, total 9, avg 75
        // 1 recepción en A 2 uds @85 => A 6, B 5, total 11, avg (9*75+2*85)/11 = 76.818182
        // 1 devolución en A 1 ud @75 histórico => A 7, B 5, total 12, avg (11*76.818182+75)/12 = 76.666667
        // Bug viejo: ReturnStock cargaba solo BranchInventories de A, totalStock visto = 6, avg habría sido (6*76.818+75)/7 = 76.558
        using var ctx = CreateDbContext();
        await SeedVariantDistributed(ctx, 75m, 5, 5);
        var svc = new InventoryIntegrationService(ctx);

        // Venta 1 en A
        var sell = await svc.DeductStock([new Common.Contracts.inventory.StockDeductionDto(VariantId, 1)], BranchA, UserId, "Test User", Guid.NewGuid());
        Assert.True(sell.IsSuccess);
        await ctx.SaveChangesAsync();

        // Recepción 2 @85 en A (simulada)
        var variant = await ctx.ProductVariants.Include(pv => pv.BranchInventories).SingleAsync(pv => pv.Id == VariantId);
        variant.RegisterPurchase(2, 85m);
        variant.AddQuantity(2, BranchA, UserId, "Test User");
        await ctx.SaveChangesAsync();

        var afterReception = await ctx.ProductVariants.Include(pv => pv.BranchInventories).SingleAsync(pv => pv.Id == VariantId);
        Assert.Equal(11, afterReception.BranchInventories.Sum(bi => bi.Stock));
        Assert.Equal(845m / 11m, afterReception.AverageCost, precision: 5);

        // Devolución 1 @75 en A
        var ret = await svc.ReturnStock([new Common.Contracts.inventory.StockReturnDto(VariantId, 1, 75m)], BranchA, UserId, "Test User", Guid.NewGuid());
        Assert.True(ret.IsSuccess);
        await ctx.SaveChangesAsync();

        var afterReturn = await ctx.ProductVariants.Include(pv => pv.BranchInventories).SingleAsync(pv => pv.Id == VariantId);
        Assert.Equal(12, afterReturn.BranchInventories.Sum(bi => bi.Stock));
        Assert.Equal(920m / 12m, afterReturn.AverageCost, precision: 4); // 76.6666

        var movement = await ctx.StockMovements.SingleAsync(m => m.MovementType == MovementType.Return);
        Assert.Equal(6, movement.StockBefore); // branch A before
        Assert.Equal(7, movement.StockAfter);
    }

    [Fact]
    public async Task Reception_InBranchA_ShouldUseGlobalStock_ForAverageCost()
    {
        // Inicial A5 B5 @75 => total 10
        // Recepción 2@85 en A => A7 B5 total 12, avg (10*75+170)/12=76.666667
        // Si usara solo stock de A (5), daría (5*75+170)/7=77.857
        using var ctx = CreateDbContext();
        await SeedVariantDistributed(ctx, 75m, 5, 5);

        var variant = await ctx.ProductVariants.Include(pv => pv.BranchInventories).SingleAsync(pv => pv.Id == VariantId);
        var before = variant.GetStockByBranch(BranchA); // 5
        variant.RegisterPurchase(2, 85m);
        variant.AddQuantity(2, BranchA, UserId, "Test User");
        var after = variant.GetStockByBranch(BranchA); // 7
        ctx.StockMovements.Add(StockMovement.CreateReception(BranchA, VariantId, UserId, "Test User", 2, Guid.NewGuid(), 85m, before, after));
        await ctx.SaveChangesAsync();

        var afterReception = await ctx.ProductVariants.Include(pv => pv.BranchInventories).SingleAsync(pv => pv.Id == VariantId);
        Assert.Equal(12, afterReception.BranchInventories.Sum(bi => bi.Stock));
        Assert.Equal(920m / 12m, afterReception.AverageCost, precision: 4);
        var movement = await ctx.StockMovements.SingleAsync(m => m.MovementType == MovementType.Reception);
        Assert.Equal(5, movement.StockBefore);
        Assert.Equal(7, movement.StockAfter);
    }

    [Fact]
    public async Task Revert_InBranchA_ShouldUseGlobalStock_ForAverageCost()
    {
        // Inicial A5 B5 @75 + recepción 10@50 en A => A15 B5 total 20, avg (10*75+500)/20=62.5? Luego revert 10@50 => vuelve a 10 total avg 75
        // Usamos números simples: A5 B5 @20, recepción 10@50 en A => A15 B5 total20 avg (10*20+500)/20=35
        // Revert 10@50 => total10 avg 20
        using var ctx = CreateDbContext();
        await SeedVariantDistributed(ctx, 20m, 5, 5);
        var variant = await ctx.ProductVariants.Include(pv => pv.BranchInventories).SingleAsync(pv => pv.Id == VariantId);
        variant.RegisterPurchase(10, 50m);
        variant.AddQuantity(10, BranchA, UserId, "Test User");
        await ctx.SaveChangesAsync();

        var afterReception = await ctx.ProductVariants.Include(pv => pv.BranchInventories).SingleAsync(pv => pv.Id == VariantId);
        Assert.Equal(20, afterReception.BranchInventories.Sum(bi => bi.Stock));
        Assert.Equal(35m, afterReception.AverageCost, precision: 4);

        // Revert 10@50 en A
        var before = afterReception.GetStockByBranch(BranchA); // 15
        variant = await ctx.ProductVariants.Include(pv => pv.BranchInventories).SingleAsync(pv => pv.Id == VariantId);
        variant.RevertPurchase(10, 50m);
        variant.RemoveQuantity(10, BranchA);
        var after = variant.GetStockByBranch(BranchA); // 5
        ctx.StockMovements.Add(StockMovement.CreateReceptionRevert(BranchA, VariantId, UserId, "Test User", 10, Guid.NewGuid(), 50m, before, after));
        await ctx.SaveChangesAsync();

        var afterRevert = await ctx.ProductVariants.Include(pv => pv.BranchInventories).SingleAsync(pv => pv.Id == VariantId);
        Assert.Equal(10, afterRevert.BranchInventories.Sum(bi => bi.Stock));
        Assert.Equal(20m, afterRevert.AverageCost, precision: 4);
        var movement = await ctx.StockMovements.SingleAsync(m => m.MovementType == MovementType.ReceptionRevert);
        Assert.Equal(15, movement.StockBefore);
        Assert.Equal(5, movement.StockAfter);
    }
}
