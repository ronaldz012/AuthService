using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Transfers.Create;

public static class CreateStockTransferErrors
{
    public static readonly Error SameBranchTransfer = new(ErrorCode.InvalidState, "Cannot transfer to the same branch");
    public static readonly Error VariantsNotFoundInBranch = new(ErrorCode.NotFound, "Variants not found in branch");
    public static readonly Error InsufficientStock = new(ErrorCode.InvalidState, "Insufficient stock for some variants");
    public static readonly Error ProductInactive = new(ErrorCode.Conflict, "One or more products are inactive and cannot be transferred");
}
