using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.ProductVariants.GetById;
public class GetProductVariantDetails(IInvDbContext context)
{
public async Task<Result<ProductVariantDetailsDto>> Execute(ActorContext ctx, Guid id)
{
    var currentBranchId = ctx.BranchIds.First();
    
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
            Size = pv.Size.Name,
            Color = pv.Color.Name,
            Price = pv.Price,
            AverageCost = pv.AverageCost,
            CurrentStock = pv.BranchInventories
                            .Where(bi => bi.BranchId == currentBranchId)
                            .Select(bi => bi.Stock)
                            .FirstOrDefault()            
        })
        .FirstOrDefaultAsync();

    if (result is null)
        return GetProductVariantDetailsErrors.VariantNotFound;

    return result;
}
    
}
