using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;
using Npgsql;

namespace Module.Inventory.Application.UseCases.Products.Create;

public class CreateProductUc(IInvDbContext context, ITenantContext tenantContext)
{
    public async Task<Result<ProductCreatedDto>> Execute(CreateProductRequest request)
    {
        // ── Validaciones previas ──────────────────────────────────────────
        var brand = await context.Brands.FindAsync(request.BrandId);
        if (brand == null)
            return new Error("NOT_FOUND", "Brand not found");

        var colorIds = request.Variants.Select(pv => pv.ColorId).Distinct().ToList();
        var colors = await context.Colors
            .Where(c => colorIds.Contains(c.Id))
            .ToListAsync();

        if (colorIds.Except(colors.Select(c => c.Id)).Any())
            return new Error("NOT_FOUND", "One or more colors not found");

        // ── Transacción ───────────────────────────────────────────────────
        await using var tx = await context.Database.BeginTransactionAsync();
        try
        {
            // 1. Reservar código de producto en la marca
            var internalCode = await ReserveBrandCounter(request.BrandId, brand.Prefix);

            // 2. Crear producto (sin variantes todavía — necesitamos el Id)
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                CategoryId = request.CategoryId,
                BrandId = request.BrandId,
                Gender = request.Gender,
                InternalCode = internalCode,
                ProductVariantCounter = 0
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            // 3. Crear variantes con SKU secuencial por producto
            var variants = new List<ProductVariant>();
            foreach (var pv in request.Variants)
            {
                var sku = await ReserveVariantCounter(product.Id, product.InternalCode);
                variants.Add(new ProductVariant
                {
                    ProductId = product.Id,
                    ColorId = pv.ColorId,
                    Size = pv.Size,
                    Description = pv.Description,
                    Price = pv.Price,
                    Sku = sku
                });
            }

            context.ProductVariants.AddRange(variants);
            await context.SaveChangesAsync();

            await tx.CommitAsync();

            // 4. Recargar con todas las relaciones para el frontend
            var saved = await context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductVariants)
                    .ThenInclude(pv => pv.Color)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (saved == null)
                return new Error("SERVER_ERROR", "Error retrieving created product");

            return new ProductCreatedDto
            {
                Id = saved.Id,
                InternalCode = saved.InternalCode,
                Name = saved.Name,
                BrandName = saved.Brand.Name,
                CategoryName = saved.Category.Name,
                Variants = saved.ProductVariants.Select(pv => new ProductVariantsCreated
                {
                    ProductVariantId = pv.Id,
                    Sku = pv.Sku,
                    Size = pv.Size,
                    ColorName = pv.Color.Name
                }).ToList()
            };
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private async Task<string> ReserveBrandCounter(Guid brandId, string prefix)
    {
        var schema = tenantContext.Schema;
        var sql = $"""
                   UPDATE "{schema}"."Brands"
                   SET "ProductCounter" = "ProductCounter" + 1
                   WHERE "Id" = @id
                   RETURNING "ProductCounter"
                   """;

        var result = await context.Database
            .SqlQueryRaw<int>(sql, new NpgsqlParameter("id", brandId))
            .ToListAsync();

        return $"{prefix}-{result[0]}";
    }

    private async Task<string> ReserveVariantCounter(Guid productId, string productCode)
    {
        var schema = tenantContext.Schema;
        var sql = $"""
                   UPDATE "{schema}"."Products"
                   SET "ProductVariantCounter" = "ProductVariantCounter" + 1
                   WHERE "Id" = @id
                   RETURNING "ProductVariantCounter"
                   """;

        var result = await context.Database
            .SqlQueryRaw<int>(sql, new NpgsqlParameter("id", productId))
            .ToListAsync();

        return $"{productCode}-{result[0].ToString().PadLeft(3, '0')}";
    }
}