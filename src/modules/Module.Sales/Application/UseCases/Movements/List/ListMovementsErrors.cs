using Common.Utilities;

namespace Module.Sales.Application.UseCases.Movements.List;

public static class ListMovementsErrors
{
    public static readonly Error NoOpenClosure = new(ErrorCode.NotFound, "No open cash register closure found for this branch.");
}
