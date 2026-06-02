using Inventory.Contracts.Dtos;
using Inventory.Contracts.Dtos.Products;
using Inventory.Data.Entities.Products;
using Inventory.Infrastructure;
using Inventory.Infrastructure.Notifications;
using Org.BouncyCastle.Ocsp;
using Common.Result;
using Inventory.Data;
using Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.UseCases.Products;

public class CreateProductUc(InvDbContext context, ITenantContext tenantContext)
{
    public async Task<Result<ProductCreatedDto>> Execute(CreateProductDto request)
    {
        var brand = await context.Brands.FindAsync(request.BrandId);
        if (brand == null)
            return new Error("NOT_FOUND", "Brand not found");
        
        var colorIds = request.Variants.Select(pv => pv.ColorId).Distinct().ToList();
        var colors = await context.Colors.Where(c => colorIds.Contains(c.Id)).ToListAsync();
        var missingColors = colorIds.Except(colors.Select(c => c.Id)).ToList();
        if (missingColors.Any())
            return new Error("NOT_FOUND", "Colors not found");

        var internalCode = await ReserveBrandCounter(request.BrandId, brand.Prefix);

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            Gender = request.Gender,
            InternalCode = internalCode,
            ProductVariants = request.Variants.Select(pv => new ProductVariant
            {
                ColorId = pv.ColorId,
                Size = pv.Size,
                Description = pv.Description,
                Price = pv.Price,
                Sku = ProductVariant.GenerateSku(
                    internalCode, 
                    colors.First(c => c.Id == pv.ColorId).Code, 
                    pv.Size
                )
            }).ToList() 
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();
        
        // 4. Traer el producto con TODAS sus relaciones necesarias para el frontend
        var savedProduct = await context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.ProductVariants)
                .ThenInclude(pv => pv.Color) // Crucial para obtener el ColorName
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        if (savedProduct == null)
            return new Error("SERVER_ERROR", "Error retrieving created product");
        
        return new ProductCreatedDto
        {
            Id = savedProduct.Id,
            InternalCode = savedProduct.InternalCode,
            Name = savedProduct.Name,
            BrandName = savedProduct.Brand.Name,
            CategoryName = savedProduct.Category.Name,
            Variants = savedProduct.ProductVariants.Select(pv => new ProductVariantsCreated
            {
                ProductVariantId = pv.Id, 
                Sku = pv.Sku,
                Size = pv.Size,
                ColorName = pv.Color.Name 
            }).ToList()
        };
    }

   private async Task<string> ReserveBrandCounter(Guid brandId, string prefix)
    {
        var schema = tenantContext.Schema;

        var sql = $$"""
            UPDATE "{{schema}}"."Brands" 
            SET "ProductCounter" = "ProductCounter" + 1 
            WHERE "Id" = {0} 
            RETURNING "ProductCounter"
            """;

        var result = await context.Database
            .SqlQueryRaw<int>(sql, brandId)
            .ToListAsync();

        return $"{prefix}-{result[0]}";
    }
}