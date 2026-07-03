using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.ProductVariants.GetBySku;

public class GetProductVariantByCode(IInvDbContext context, ICurrentUser currentUser)
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
                ColorId = pv.ColorId,
                ColorName = pv.Color.Name,
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
        if(result is null) return GetProductVariantByCodeErrors.VariantNotFound;

        return result;
    }
}