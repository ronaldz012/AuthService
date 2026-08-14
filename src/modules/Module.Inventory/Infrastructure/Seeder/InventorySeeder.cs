using Common.Contracts.authentication;
using Common.Contracts.Seeder;
using Common.Contracts.inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Infrastructure.Seeder;

public class InventorySeeder(
    IServiceProvider serviceProvider,
    ITenantDatabaseResolver tenantResolver) : IDataSeeder
{
    public int Order => 5;

    public async Task SeedAsync()
    {
        var context = serviceProvider.GetRequiredService<IInvDbContext>();
        var codeService = serviceProvider.GetRequiredService<IProductCodeService>();
        var catalogProvisioner = serviceProvider.GetRequiredService<IDefaultCatalogProvisioner>();

        if (await context.Categories.AnyAsync()) return;

        var tenantInfo = await tenantResolver.GetByDisplayName("default");
        if (tenantInfo is null) return;

        // Catálogo base (colores, tallas, categorías) desde el template compartido
        await catalogProvisioner.SeedAsync(
            tenantInfo.TenantId,
            tenantInfo.OwnerUserId,
            "System",
            DefaultCatalogTemplates.Basic);

        // Marcas demo (cada tenant gestiona las propias)
        foreach (var brand in InventorySeedData.Brands)
        {
            if (!await context.Brands.AnyAsync(b => b.Prefix.ToLower() == brand.Prefix.ToLower()))
                context.Brands.Add(Brand.Create(brand.Name, brand.Prefix, tenantInfo.TenantId, tenantInfo.OwnerUserId, "System"));
        }

        await context.SaveChangesAsync();

        // Productos demo usando el catálogo ya sembrado (se omiten los que no existan)
        foreach (var prodSeed in InventorySeedData.Products)
        {
            var brand = await context.Brands.FirstOrDefaultAsync(b => b.Name == prodSeed.Brand);
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Name == prodSeed.Category);
            if (brand is null || category is null) continue;

            var code = await codeService.ReserveBrandCounter(brand.Id, brand.Prefix);
            var product = Product.Create(prodSeed.Name, prodSeed.Description, category.Id, brand.Id, prodSeed.Gender, code, tenantInfo.TenantId, tenantInfo.OwnerUserId, "System");
            context.Products.Add(product);
            await context.SaveChangesAsync();

            foreach (var varSeed in prodSeed.Variants)
            {
                var color = await context.Colors.FirstOrDefaultAsync(c => c.Name == varSeed.Color);
                var size = await context.Sizes.FirstOrDefaultAsync(s => s.Name == varSeed.Size);
                if (color is null || size is null) continue;

                var sku = await codeService.ReserveVariantCounter(product.Id, product.InternalCode);
                context.ProductVariants.Add(ProductVariant.Create(product.Id, color.Id, size.Id, varSeed.Price, sku, tenantInfo.TenantId, tenantInfo.OwnerUserId, "System"));
            }
        }

        await context.SaveChangesAsync();
    }
}