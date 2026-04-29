using Auth.Contracts.Interfaces;
using Inventory.Data.Entities.Transfers;
using Inventory.Data.Persistence;
using Shared.Result;

namespace Inventory.UseCases.Transfers;

public class CancelStockTransfer(InvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<bool>> Execute(int transferId)
    {
        var transfer = await context.StockTransfers.FindAsync(transferId);
        if (transfer == null) return new Error("NOT_FOUND", "Transfer not found");
        
        if(transfer.FromBranchId != currentUser.BranchIds[0]) 
            return new Error("VALIDATION_ERROR", "Cannot delete from different branch");
        
        if(transfer.Status != TransferStatus.Pending ) return new Error("VALIDATION_ERROR", "cannot delete Transfer not pending");
        
        transfer.DeletedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }
}