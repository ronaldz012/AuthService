using Common.Utilities;

namespace Module.Sales.Application.UseCases.Movements.Create;

public static class CreateMovementErrors
{
    public static readonly Error ClosureNotFound = new(ErrorCode.NotFound, "Cash register closure not found or does not belong to this branch.");
    public static readonly Error ClosureNotOpen = new(ErrorCode.Conflict, "Cash register closure must be open to register movements.");
    public static readonly Error Failed = new(ErrorCode.InternalError, "Could not create movement.");
}
