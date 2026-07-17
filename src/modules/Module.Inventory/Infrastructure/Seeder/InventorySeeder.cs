using Common.Contracts.authentication;
using Common.Contracts.Seeder;
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

        if (await context.Categories.AnyAsync()) return;

        var tenantInfo = await tenantResolver.GetByDisplayName("default");
        if (tenantInfo is null) return;

        foreach (var name in InventorySeedData.Colors)
        {
            if (!await context.Colors.AnyAsync(c => c.Name == name))
                context.Colors.Add(new Color
                {
                    Name = name,
                    CreatedById = tenantInfo.OwnerUserId
                });
        }

        foreach (var name in InventorySeedData.Categories)
        {
            if (!await context.Categories.AnyAsync(c => c.Name == name))
                context.Categories.Add(new Category { Name = name });
        }

        foreach (var brand in InventorySeedData.Brands)
        {
            if (!await context.Brands.AnyAsync(b => b.Prefix == brand.Prefix))
                context.Brands.Add(new Brand { Name = brand.Name, Prefix = brand.Prefix });
        }

        await context.SaveChangesAsync();

        foreach (var prodSeed in InventorySeedData.Products)
        {
            var brand = await context.Brands.FirstAsync(b => b.Name == prodSeed.Brand);
            var category = await context.Categories.FirstAsync(c => c.Name == prodSeed.Category);

            var code = await context.ReserveBrandCounter(brand.Id, brand.Prefix);
            var product = new Product
            {
                Name = prodSeed.Name,
                Description = prodSeed.Description,
                CategoryId = category.Id,
                BrandId = brand.Id,
                Gender = prodSeed.Gender,
                InternalCode = code,
                ProductVariantCounter = 0
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            foreach (var varSeed in prodSeed.Variants)
            {
                var color = await context.Colors.FirstAsync(c => c.Name == varSeed.Color);
                var sku = await context.ReserveVariantCounter(product.Id, product.InternalCode);
                context.ProductVariants.Add(new ProductVariant
                {
                    ProductId = product.Id,
                    ColorId = color.Id,
                    Size = varSeed.Size,
                    Price = varSeed.Price,
                    Sku = sku
                });
            }
        }

        await context.SaveChangesAsync();
    }
}
