using Inventory.Data.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Common.Result;
using Inventory.Data;

namespace Inventory.UseCases.Products;

public class ValidateProductVariants(InvDbContext context)
{
    public async Task<Result<List<ProductVariant>>> Execute(List<Guid> variantIds)
    {
        var productVariants = await context.ProductVariants.Where(x => variantIds.Contains(x.Id)).ToListAsync();

        var missingIds = productVariants.Except(productVariants).ToList();
        if (missingIds.Count != 0)
            return new Error("NOT_FOUND", $"the next Ids of VariantIds were Not founded: {missingIds}");
        return productVariants;
    }
}