using Common.Contracts.authentication;
using Common.Utilities;
using Inventory.Contracts.Dtos.ProductVariants;
using Inventory.Data.Entities.Inventory;
using Inventory.Data.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Inventory.Data;

namespace Inventory.UseCases.ProductVariants;

public class CorrectProductVariantStock(InvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<bool>> Execute(UpdateProductVariantStockDto request, Guid id)
    {
        var currentBranch = currentUser.BranchIds.FirstOrDefault();
        
        var pv = await context.ProductVariants
            .Where(p => p.Id == id)
            .Include(p => p.BranchInventories.Where(bi => bi.BranchId == currentBranch))
            .FirstOrDefaultAsync();
    
        if (pv == null) 
            return new Error("NOT_FOUND", "Product Variant not found");
    
        try 
        {
            pv.CorrectQuantity(request.Stock, currentBranch);
            var movement = StockMovement.CreateAdjustment(
                currentBranch, 
                pv.Id, 
                currentUser.UserId, 
                request.Stock, 
                request.Notes);
            context.StockMovements.Add(movement);
            await context.SaveChangesAsync();
            return true;
        }
        catch (InvalidOperationException ex)
        {
            return new Error("VALIDATION_ERROR", ex.Message);
        }
    }
}