using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Transfers.Cancel;

public static class CancelStockTransferErrors
{
    public static readonly Error TransferNotFound = new(ErrorCode.NotFound, "Transfer not found");
    public static readonly Error DifferentBranch = new(ErrorCode.ValidationError, "Cannot delete from different branch");
    public static readonly Error NotPending = new(ErrorCode.ValidationError, "Cannot delete Transfer not pending");
}
