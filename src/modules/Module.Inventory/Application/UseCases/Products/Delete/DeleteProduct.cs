using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Products.Delete;

public class DeleteProduct(IInvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<bool>> Execute(Guid id)
    {

        var hasStock = await context.ProductVariants
            .Where(v => v.ProductId == id)
            .SelectMany(v => v.BranchInventories)
            .AnyAsync(bi => bi.Stock > 0);

        if (hasStock) 
            return new Error("VALIDATION_ERROR", "Inventory still available");
        
        
        var affectedRows = await context.Products
            .Where(p => p.Id == id && p.DeletedAt == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.DeletedAt, DateTime.UtcNow)
                .SetProperty(p => p.DeletedById, currentUser.UserId));

        if (affectedRows == 0) 
            return new Error("NOT_FOUND", "Product not found");

        return true;
    }
}