using Microsoft.EntityFrameworkCore;
using Common.Result;
using Inventory.Data;

namespace Inventory.UseCases.Products;

public class ValidateProducts(InvDbContext context)
{
    public async Task<Result<bool>> Execute(List<Guid> productIds)
    {
        var idsFounded = await context.Products.Where(x => productIds.Contains(x.Id)).Select(x => x.Id).ToListAsync();

        var missingIds = productIds.Except(idsFounded).ToList();
        if (missingIds.Any())
            return new Error("NOT_FOUND", $"the next Ids Where Not founded{missingIds}");
        return true;
    }
}