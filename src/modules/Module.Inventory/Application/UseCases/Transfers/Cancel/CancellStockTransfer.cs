using Common.Contracts.authentication;
using Common.Utilities;
using Inventory.Data.Entities.Transfers;
using Inventory.Data;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Transfers;

namespace Inventory.UseCases.Transfers;

public class CancelStockTransfer(IInvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<bool>> Execute(int transferId)
    {
        var transfer = await context.StockTransfers.FindAsync(transferId);
        if (transfer == null) return new Error("NOT_FOUND", "Transfer not found");

        if (transfer.FromBranchId != currentUser.BranchIds[0])
            return new Error("VALIDATION_ERROR", "Cannot delete from different branch");

        if (transfer.Status != TransferStatus.Pending) return new Error("VALIDATION_ERROR", "cannot delete Transfer not pending");

        transfer.Status = TransferStatus.Cancelled;
        await context.SaveChangesAsync();
        return true;
    }
}