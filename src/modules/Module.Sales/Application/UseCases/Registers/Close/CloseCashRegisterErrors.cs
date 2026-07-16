using Common.Utilities;

namespace Module.Sales.Application.UseCases.Registers.Close;

public static class CloseCashRegisterErrors
{
    public static readonly Error NotFound = new(ErrorCode.NotFound, "No open cash register found for this branch.");
    public static readonly Error AlreadyClosed = new(ErrorCode.Conflict, "Cash register is already closed.");
}
