using Common.Contracts.authentication;
using Common.Utilities;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Transfers;

namespace Module.Inventory.Application.UseCases.Transfers.Cancel;

public class CancelStockTransfer(IInvDbContext context)
{
    public async Task<Result<bool>> Execute(ActorContext ctx, Guid transferId)
    {
        var transfer = await context.StockTransfers.FindAsync(transferId);
        if (transfer == null) return CancelStockTransferErrors.TransferNotFound;

        if (transfer.FromBranchId != ctx.BranchId)
            return CancelStockTransferErrors.DifferentBranch;

        if (transfer.Status != TransferStatus.Pending) return CancelStockTransferErrors.NotPending;

        transfer.Cancel(ctx.UserId, ctx.FullName);
        await context.SaveChangesAsync();
        return true;
    }
}