using Common.Contracts.authentication;
using Common.Utilities;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Transfers;

namespace Module.Inventory.Application.UseCases.Transfers.Cancel;

public class CancelStockTransfer(IInvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<bool>> Execute(int transferId)
    {
        var transfer = await context.StockTransfers.FindAsync(transferId);
        if (transfer == null) return CancelStockTransferErrors.TransferNotFound;

        if (transfer.FromBranchId != currentUser.BranchIds[0])
            return CancelStockTransferErrors.DifferentBranch;

        if (transfer.Status != TransferStatus.Pending) return CancelStockTransferErrors.NotPending;

        transfer.Cancel(currentUser.UserId, currentUser.FullName);
        await context.SaveChangesAsync();
        return true;
    }
}