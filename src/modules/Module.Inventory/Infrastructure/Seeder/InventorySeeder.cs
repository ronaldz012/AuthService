using Common.Contracts.authentication;
using Common.Contracts.Seeder;
using Common.Contracts.inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Application.UseCases.Brands.CreateBrand;
using Module.Inventory.Application.UseCases.Products.Create;

namespace Module.Inventory.Infrastructure.Seeder;

public class InventorySeeder(
    IServiceProvider serviceProvider,
    ITenantDatabaseResolver tenantResolver) : IDataSeeder
{
    public int Order => 5;

    public async Task SeedAsync()
    {
        var context = serviceProvider.GetRequiredService<IInvDbContext>();
        var catalogProvisioner = serviceProvider.GetRequiredService<IDefaultCatalogProvisioner>();
        var createBrand = serviceProvider.GetRequiredService<CreateBrandUc>();
        var createProduct = serviceProvider.GetRequiredService<CreateProductUc>();

        if (await context.Categories.AnyAsync()) return;

        var tenantInfo = await tenantResolver.GetByDisplayName("default");
        if (tenantInfo is null) return;

        // Catálogo base (colores, tallas, categorías) desde el template compartido
        await catalogProvisioner.SeedAsync(
            tenantInfo.TenantId,
            tenantInfo.OwnerUserId,
            "System",
            DefaultCatalogTemplates.Basic);

        var actor = new ActorContext(tenantInfo.TenantId, tenantInfo.OwnerUserId, "System", Guid.Empty, []);

        // Marcas demo (cada tenant gestiona las propias)
        foreach (var brand in InventorySeedData.Brands)
        {
            if (await context.Brands.AnyAsync(b => b.Prefix.ToLower() == brand.Prefix.ToLower())) continue;

            var brandResult = await createBrand.Execute(actor, new CreateBrandRequest
            {
                Name = brand.Name,
                Prefix = brand.Prefix,
                Description = string.Empty
            });

            if (!brandResult.IsSuccess)
                throw new InvalidOperationException(
                    $"Seeding brand {brand.Name} failed: {brandResult.Error?.Code} - {brandResult.Error?.Message}");
        }

        // Productos demo usando el catálogo ya sembrado (se omiten los que no existan)
        foreach (var prodSeed in InventorySeedData.Products)
        {
            var brand = await context.Brands.FirstOrDefaultAsync(b => b.Name == prodSeed.Brand);
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Name == prodSeed.Category);
            if (brand is null || category is null) continue;

            var variants = new List<CreateProductVariantForProductDto>();
            foreach (var varSeed in prodSeed.Variants)
            {
                var color = await context.Colors.FirstOrDefaultAsync(c => c.Name == varSeed.Color);
                var size = await context.Sizes.FirstOrDefaultAsync(s => s.Name == varSeed.Size);
                if (color is null || size is null) continue;

                variants.Add(new CreateProductVariantForProductDto
                {
                    ColorId = color.Id,
                    SizeId = size.Id,
                    Price = varSeed.Price,
                    Description = string.Empty
                });
            }

            if (variants.Count == 0) continue;

            var productResult = await createProduct.Execute(actor, new CreateProductRequest
            {
                Name = prodSeed.Name,
                Description = prodSeed.Description,
                CategoryId = category.Id,
                BrandId = brand.Id,
                Gender = prodSeed.Gender,
                Variants = variants
            });

            if (!productResult.IsSuccess)
                throw new InvalidOperationException(
                    $"Seeding product {prodSeed.Name} failed: {productResult.Error?.Code} - {productResult.Error?.Message}");
        }
    }
}