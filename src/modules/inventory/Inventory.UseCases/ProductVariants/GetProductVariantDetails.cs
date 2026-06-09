using Auth.Contracts.Interfaces;
using Inventory.Contracts.Dtos.Products;
using Microsoft.EntityFrameworkCore;
using Common.Result;
using Inventory.Data;
using Inventory.Contracts.Dtos.ProductVariants;

namespace Inventory.UseCases.ProductVariants;
public class GetProductVariantDetails(InvDbContext context, ICurrentUser currentUser)
{
public async Task<Result<ProductVariantDetailsDto>> Execute(Guid id)
{
    var currentBranchId = currentUser.BranchIds.First();
    
    var result = await context.ProductVariants
        .Where(pv => pv.Id == id)
        .Select(pv => new ProductVariantDetailsDto
        {
            Id = pv.Id,
            ProductId = pv.ProductId,
            ProductName = pv.Product.Name,
            ProductCategory = pv.Product.Category.Name,
            ProductBrand = pv.Product.Brand.Name,
            Sku = pv.Sku,
            Description = pv.Description,
            Size = pv.Size,
            Color = pv.Color.Name,
            Price = pv.Price,
            CurrentStock = pv.BranchInventories
                            .Where(bi => bi.BranchId == currentBranchId)
                            .Select(bi => bi.Stock)
                            .FirstOrDefault()            
        })
        .FirstOrDefaultAsync();

    if (result is null)
        return new Error("NOT FOUND", "productVariantNotFount");

    return result;
}
    
}
