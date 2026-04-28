using Auth.Contracts.Interfaces;
using Inventory.Contracts.Dtos.Products;
using Inventory.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

namespace Inventory.UseCases.Products;

public class GetProductByCode(InvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<ProductVariantBySkuDto>> Execute(string skuRequested)
    {
        var branch = currentUser.BranchIds[0];
        var result = await context.ProductVariants.Select(pv => new ProductVariantBySkuDto
            {
                Id = pv.Id,
                Sku = pv.Sku,
                Description = pv.Description,
                Size = pv.Size,
                Color = pv.Color,
                Price = pv.Price,
                BranchId = branch,
                AvailableStockInBranch = pv.BranchInventories.Where(bi => bi.BranchId == branch).Select(bi => bi.Stock).FirstOrDefault(),
                ProductId= pv.ProductId,
                ProductName = pv.Product.Name,
                ProductDescription = pv.Product.Description,
                Gender = pv.Product.Gender,
                BranchName = pv.Product.Brand.Name,
                CategoryName = pv.Product.Category.Name,
            }

        ).FirstOrDefaultAsync(pv => pv.Sku == skuRequested);
        if(result is null) return new Error("NOT_FOUND", "product Variant not found");

        return result;
    }
}