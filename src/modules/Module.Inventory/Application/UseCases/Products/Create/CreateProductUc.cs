using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Products.Create;

public class CreateProductUc(IInvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<ProductCreatedDto>> Execute(CreateProductRequest request)
    {
        var brand = await context.Brands.FindAsync(request.BrandId);
        if (brand == null)
            return CreateProductErrors.BrandNotFound;

        var colorIds = request.Variants.Select(pv => pv.ColorId).Distinct().ToList();
        var colors = await context.Colors
            .Where(c => colorIds.Contains(c.Id))
            .ToListAsync();

        if (colorIds.Except(colors.Select(c => c.Id)).Any())
            return CreateProductErrors.ColorsNotFound;

        await using var tx = await context.Database.BeginTransactionAsync();
        try
        {
            var internalCode = await context.ReserveBrandCounter(request.BrandId, brand.Prefix);

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                CategoryId = request.CategoryId,
                BrandId = request.BrandId,
                Gender = request.Gender,
                InternalCode = internalCode,
                ProductVariantCounter = 0,
                CreatedBy = currentUser.UserId,
                CreatedByName = currentUser.FullName
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var variants = new List<ProductVariant>();
            foreach (var pv in request.Variants)
            {
                var sku = await context.ReserveVariantCounter(product.Id, product.InternalCode);
                variants.Add(new ProductVariant
                {
                    ProductId = product.Id,
                    ColorId = pv.ColorId,
                    Size = pv.Size,
                    Description = pv.Description,
                    Price = pv.Price,
                    Sku = sku,
                    CreatedBy = currentUser.UserId,
                    CreatedByName = currentUser.FullName
                });
            }

            context.ProductVariants.AddRange(variants);
            await context.SaveChangesAsync();

            await tx.CommitAsync();

            var saved = await context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductVariants)
                    .ThenInclude(pv => pv.Color)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (saved == null)
                return CreateProductErrors.ProductRetrievalFailed;

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
}