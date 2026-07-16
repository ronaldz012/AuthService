using Common.Utilities;

namespace Module.Sales.Application.UseCases.Movements.Update;

public static class UpdateMovementErrors
{
    public static readonly Error NotFound = new(ErrorCode.NotFound, "Movement not found.");
    public static readonly Error ClosureClosed = new(ErrorCode.Conflict, "Cannot modify movements on a closed cash register.");
}
