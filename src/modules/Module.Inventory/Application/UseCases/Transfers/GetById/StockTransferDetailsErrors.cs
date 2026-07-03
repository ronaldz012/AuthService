using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Transfers.GetById;

public static class StockTransferDetailsErrors
{
    public static readonly Error TransferNotFound = new(ErrorCode.NotFound, "StockTransfer not found");
    public static readonly Error BranchesNotFound = new(ErrorCode.NotFound, "Branches not found");
    public static readonly Error UsersNotFound = new(ErrorCode.NotFound, "Users not found");
}
