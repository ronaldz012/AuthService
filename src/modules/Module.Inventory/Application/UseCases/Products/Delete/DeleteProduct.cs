using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Products.Delete;

public class DeleteProduct(IInvDbContext context)
{
    public async Task<Result<bool>> Execute(ActorContext ctx, Guid id)
    {

        var hasStock = await context.ProductVariants
            .Where(v => v.ProductId == id)
            .SelectMany(v => v.BranchInventories)
            .AnyAsync(bi => bi.Stock > 0);

        if (hasStock) 
            return DeleteProductErrors.InventoryStillAvailable;
        
        
        var affectedRows = await context.Products
            .Where(p => p.Id == id && p.DeletedAt == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.DeletedAt, DateTime.UtcNow)
                .SetProperty(p => p.DeletedBy, ctx.UserId)
                .SetProperty(p => p.DeletedByName, ctx.FullName));

        if (affectedRows == 0) 
            return DeleteProductErrors.ProductNotFound;

        return true;
    }
}