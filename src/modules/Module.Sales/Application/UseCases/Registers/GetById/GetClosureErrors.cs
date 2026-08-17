using Common.Utilities;

namespace Module.Sales.Application.UseCases.Registers.GetById;

public static class GetClosureErrors
{
    public static readonly Error NotFound = new(ErrorCode.NotFound, "Cash register closure not found.");
    public static readonly Error NoActiveClosure = new(ErrorCode.NotFound, "There is no open cash register in this branch.");
}
