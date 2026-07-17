using Common.Utilities;

namespace Module.Sales.Application.UseCases.Movements.Create;

public static class CreateMovementErrors
{
    public static readonly Error NoOpenClosure = new(ErrorCode.NotFound, "No open cash register found for this branch.");
}
