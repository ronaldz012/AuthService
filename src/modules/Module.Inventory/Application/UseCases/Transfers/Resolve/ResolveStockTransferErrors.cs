using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Transfers.Resolve;

public static class ResolveStockTransferErrors
{
    public static readonly Error TransferNotFound = new(ErrorCode.NotFound, "Transfer not found");
    public static readonly Error Forbidden = new(ErrorCode.Forbidden, "Only the destination branch can resolve this transfer");
    public static readonly Error AlreadyResolved = new(ErrorCode.InvalidState, "Transfer is no longer pending");
    public static readonly Error InsufficientStock = new(ErrorCode.InvalidState, "Insufficient stock in origin branch, transfer cannot be completed");
}
