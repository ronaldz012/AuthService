using Common.Contracts.authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Module.Inventory.Domain.Inventory;
using Module.Inventory.Domain.Products;
using Module.Inventory.Infrastructure;
using System.Infrastructure.Persistence;

namespace Test.Integration;

public class AvcoReturnIntegrationTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid BranchId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid VariantId = Guid.NewGuid();

    private static AppDbContext CreateDbContext()
    {
        var tenantCtx = TestIntegrationDbContextFactory.CreateTenantContext(TenantId);
        return TestIntegrationDbContextFactory.Create(tenantCtx);
    }

    private static ActorContext CreateActorContext() =>
        new(TenantId, UserId, "Test User", BranchId, [BranchId]);

    private static async Task SeedVariant(AppDbContext ctx, decimal avgCost, int stock)
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

        ctx.BranchInventories.Add(new BranchInventory
        {
            ProductVariantId = VariantId,
            BranchId = BranchId,
            TenantId = TenantId,
            Stock = stock,
            CreatedBy = UserId,
            CreatedByName = "Test User"
        });
        await ctx.SaveChangesAsync();
    }

    [Fact]
    public async Task Repro_UserReport_ADI001_ReturnShouldMatchControl()
    {
        // 0. Inicial: ADI1-001 a 75 Bs 10 Unid
        using var ctx = CreateDbContext();
        await SeedVariant(ctx, 75m, 10);
        var svc = new InventoryIntegrationService(ctx);

        // 1. Venta 1 a 75 (no cambia avg)
        var sell = await svc.DeductStock(
            [new Common.Contracts.inventory.StockDeductionDto(VariantId, 1)],
            BranchId, UserId, "Test User", Guid.NewGuid());
        Assert.True(sell.IsSuccess);
        await ctx.SaveChangesAsync();

        var afterSale = await ctx.ProductVariants.Include(pv => pv.BranchInventories).SingleAsync(pv => pv.Id == VariantId);
        Assert.Equal(9, afterSale.BranchInventories.Sum(bi => bi.Stock));
        Assert.Equal(75m, afterSale.AverageCost);

        // 2. Recepción 2 a 85 => (9*75 + 2*85)/11 = 76.818181...
        var variant = await ctx.ProductVariants.Include(pv => pv.BranchInventories).SingleAsync(pv => pv.Id == VariantId);
        variant.RegisterPurchase(2, 85m);
        variant.AddQuantity(2, BranchId, UserId, "Test User");
        await ctx.SaveChangesAsync();

        var afterReception = await ctx.ProductVariants.Include(pv => pv.BranchInventories).SingleAsync(pv => pv.Id == VariantId);
        Assert.Equal(11, afterReception.BranchInventories.Sum(bi => bi.Stock));
        Assert.Equal(845m / 11m, afterReception.AverageCost, precision: 5); // 76.81818
        Assert.Equal(76.818182m, Math.Round(afterReception.AverageCost, 6));

        // 3. Devolución 1 a 75 histórico => (11*76.818181 + 75)/12 = 76.666666...
        var ret = await svc.ReturnStock(
            [new Common.Contracts.inventory.StockReturnDto(VariantId, 1, 75m)],
            BranchId, UserId, "Test User", Guid.NewGuid());
        Assert.True(ret.IsSuccess);
        await ctx.SaveChangesAsync();

        var afterReturn = await ctx.ProductVariants.Include(pv => pv.BranchInventories).SingleAsync(pv => pv.Id == VariantId);
        Assert.Equal(12, afterReturn.BranchInventories.Sum(bi => bi.Stock));
        // Este es el assert que reproduce el bug reportado: esperamos 76.666, el sistema daba 76.59
        Assert.Equal(920m / 12m, afterReturn.AverageCost, precision: 4); // 76.6666
        Assert.Equal(76.666667m, Math.Round(afterReturn.AverageCost, 6));

        // Control: si venta nunca hubiera ocurrido, 10@75 + 2@85 = 12 uds, (750+170+75?) No, control es 5@10+3@15 ejemplo anterior
        // Para este caso, control equivalente es: stock inicial 10@75 + recepción 2@85 = 12@76.666? No, sin venta sería 12@76.66 igual? Verif: (10*75 +2*85)/12 =920/12=76.66 mismo que con venta+devolución → confirma que devolución netea correctamente
        var controlAvg = (10m * 75m + 2m * 85m) / 12m;
        Assert.Equal(controlAvg, afterReturn.AverageCost, precision: 4);
    }

    [Fact]
    public async Task Control_WithoutSale_ShouldMatchReturnResult()
    {
        using var ctx = CreateDbContext();
        await SeedVariant(ctx, 75m, 10);
        var variant = await ctx.ProductVariants.Include(pv => pv.BranchInventories).SingleAsync(pv => pv.Id == VariantId);
        // Sin venta, solo recepción 2@85
        variant.RegisterPurchase(2, 85m);
        variant.AddQuantity(2, BranchId, UserId, "Test User");
        await ctx.SaveChangesAsync();

        var control = await ctx.ProductVariants.SingleAsync(pv => pv.Id == VariantId);
        Assert.Equal(12, control.BranchInventories.Sum(bi => bi.Stock));
        Assert.Equal(920m / 12m, control.AverageCost, precision: 4);
    }
}
