using Common.Contracts.authentication;
using Common.Contracts.Seeder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Infrastructure.Seeder;

public class InventorySeeder(
    IServiceProvider serviceProvider,
    ITenantDatabaseResolver tenantResolver,
    IProductCodeService codeService) : IDataSeeder
{
    public int Order => 5;

    public async Task SeedAsync()
    {
        var context = serviceProvider.GetRequiredService<IInvDbContext>();

        if (await context.Categories.AnyAsync()) return;

        var tenantInfo = await tenantResolver.GetByDisplayName("default");
        if (tenantInfo is null) return;

        foreach (var name in InventorySeedData.Colors)
        {
            if (!await context.Colors.AnyAsync(c => c.Name == name))
                context.Colors.Add(Color.Create(name, tenantInfo.TenantId, tenantInfo.OwnerUserId));
        }

        foreach (var name in InventorySeedData.Categories)
        {
            if (!await context.Categories.AnyAsync(c => c.Name == name))
                context.Categories.Add(Category.Create(name, tenantInfo.TenantId, tenantInfo.OwnerUserId, "System"));
        }

        foreach (var brand in InventorySeedData.Brands)
        {
            if (!await context.Brands.AnyAsync(b => b.Prefix == brand.Prefix))
                context.Brands.Add(Brand.Create(brand.Name, brand.Prefix, tenantInfo.TenantId, tenantInfo.OwnerUserId, "System"));
        }

        await context.SaveChangesAsync();

        foreach (var prodSeed in InventorySeedData.Products)
        {
            var brand = await context.Brands.FirstAsync(b => b.Name == prodSeed.Brand);
            var category = await context.Categories.FirstAsync(c => c.Name == prodSeed.Category);

            var code = await codeService.ReserveBrandCounter(brand.Id, brand.Prefix);
            var product = Product.Create(prodSeed.Name, prodSeed.Description, category.Id, brand.Id, prodSeed.Gender, code, tenantInfo.TenantId, tenantInfo.OwnerUserId, "System");
            context.Products.Add(product);
            await context.SaveChangesAsync();

            foreach (var varSeed in prodSeed.Variants)
            {
                var color = await context.Colors.FirstAsync(c => c.Name == varSeed.Color);
                var sku = await codeService.ReserveVariantCounter(product.Id, product.InternalCode);
                context.ProductVariants.Add(ProductVariant.Create(product.Id, color.Id, varSeed.Size, varSeed.Price, sku, tenantInfo.TenantId, tenantInfo.OwnerUserId, "System"));
            }
        }

        await context.SaveChangesAsync();
    }
}
