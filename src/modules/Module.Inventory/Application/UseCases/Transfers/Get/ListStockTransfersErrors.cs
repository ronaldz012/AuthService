using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Transfers.Get;

public static class ListStockTransfersErrors
{
    public static readonly Error BranchLookupFailed = new(ErrorCode.InternalError, "Failed to resolve branch names");
    public static readonly Error UserLookupFailed = new(ErrorCode.InternalError, "Failed to resolve user names");
}
