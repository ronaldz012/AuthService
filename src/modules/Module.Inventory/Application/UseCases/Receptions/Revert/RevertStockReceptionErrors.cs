using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Receptions.Revert;

public static class RevertStockReceptionErrors
{
    public static readonly Error ReceptionNotFound = new(ErrorCode.NotFound, "Reception not found");
    public static readonly Error AlreadyReverted = new(ErrorCode.InvalidState, "Reception has already been reverted");
    public static readonly Error Outdated = new(ErrorCode.Conflict, "Reception is older than the allowed period and cannot be reverted");
    public static readonly Error NotEnoughStock = new(ErrorCode.Conflict, "Insufficient stock in the branch to revert the reception");
    public static readonly Error RevertFailed = new(ErrorCode.InternalError, "Failed to revert reception");
}