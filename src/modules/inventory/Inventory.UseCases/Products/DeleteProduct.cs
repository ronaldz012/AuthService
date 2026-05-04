using System.Security.Cryptography.X509Certificates;
using Auth.Contracts.Interfaces;
using Inventory.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

namespace Inventory.UseCases.Products;

public class DeleteProduct(InvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<bool>> Execute(int id)
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