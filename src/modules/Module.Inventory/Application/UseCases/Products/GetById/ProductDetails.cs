using Common.Contracts.authentication;
using Common.Utilities;
using Inventory.Contracts.Dtos.Products;
using Microsoft.EntityFrameworkCore;
using Inventory.Data;

namespace Inventory.UseCases.Products;

public class ProductDetails(InvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<ProductDetailDto>> Execute(Guid productId)
    {
        var currentBranch = currentUser.BranchIds[0];
        var branchNames = await currentUser.GetBranchNamesAsync();

        var result = await context.Products.Select(p => new ProductDetailDto
        {
            Id = p.Id,
            Name = p.Name,
            InternalCode = p.InternalCode,
            Description = p.Description,
            BasePrice = p.BasePrice,
            Gender = p.Gender,
            CategoryId = p.CategoryId,
            CategoryName = p.Category.Name,
            BrandId = p.BrandId,
            BrandName = p.Brand.Name,
            Variants = p.ProductVariants.Select(pv => new ProductVarianListDto
            {
                Id = pv.Id,
                Sku = pv.Sku,
                Description = pv.Description,
                Size = pv.Size,
                Color = pv.Color.Name,
                ColorId = pv.ColorId,
                Price = pv.Price,
                Stock = pv.BranchInventories
                    .Where(x => x.BranchId == currentBranch)
                    .Select(x => (int?)x.Stock) // Casteo preventivo a nullable
                    .FirstOrDefault() ?? 0,
            }).ToList()
        }).FirstOrDefaultAsync(x => x.Id == productId);


        if (result is null) return new Error("NOT_FOUND", "Product not found");

        result.TotalStock = result.Variants.Sum(x => x.Stock);
        return result;
    }
}