using Common.Utilities;

namespace Module.Inventory.Application.UseCases.StockMovements.Get;

public static class ListStockMovementsErrors
{
    public static readonly Error BranchLookupFailed = new(ErrorCode.InternalError, "Failed to resolve branch names");
    public static readonly Error UserLookupFailed = new(ErrorCode.InternalError, "Failed to resolve user names");
}
