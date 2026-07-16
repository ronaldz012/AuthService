using Common.Utilities;

namespace Module.Sales.Application.UseCases.Movements.Delete;

public static class DeleteMovementErrors
{
    public static readonly Error NotFound = new(ErrorCode.NotFound, "Movement not found.");
    public static readonly Error ClosureClosed = new(ErrorCode.Conflict, "Cannot delete movements from a closed cash register.");
}
