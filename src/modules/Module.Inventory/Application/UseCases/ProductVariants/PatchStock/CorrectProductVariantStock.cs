using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Inventory;

namespace Module.Inventory.Application.UseCases.ProductVariants.PatchStock;

public class CorrectProductVariantStock(IInvDbContext context)
{
    public async Task<Result<bool>> Execute(ActorContext ctx, UpdateProductVariantStockDto request, Guid id)
    {
        var currentBranch = ctx.BranchIds.FirstOrDefault();
        
        var pv = await context.ProductVariants
            .Where(p => p.Id == id)
            .Include(p => p.BranchInventories.Where(bi => bi.BranchId == currentBranch))
            .FirstOrDefaultAsync();
    
        if (pv == null) 
            return CorrectProductVariantStockErrors.VariantNotFound;
    
        try 
        {
            var existingInventory = pv.BranchInventories.FirstOrDefault(bi => bi.BranchId == currentBranch);
            var previousStock = existingInventory?.Stock ?? 0;
            var delta = request.Stock - previousStock;

            if (delta > 0)
                return CorrectProductVariantStockErrors.SurplusNotAllowed;

            // Se valida el movimiento primero (CreateAdjustment exige Notes); si falla, no se muta stock
            var movement = delta != 0
                ? StockMovement.CreateAdjustment(
                    currentBranch,
                    pv.Id,
                    ctx.UserId,
                    ctx.FullName,
                    delta,
                    request.Notes,
                    pv.AverageCost,
                    previousStock,
                    request.Stock)
                : null;

            pv.CorrectQuantity(request.Stock, currentBranch, ctx.UserId, ctx.FullName);

            if (movement is not null)
                context.StockMovements.Add(movement);

            await context.SaveChangesAsync();
            return true;
        }
        catch (InvalidOperationException)
        {
            return CorrectProductVariantStockErrors.StockCorrectionFailed;
        }
    }
}