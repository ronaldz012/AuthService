using Common.Utilities;

namespace Module.Sales.Application.UseCases.Sales.Create;

public static class CreateSaleErrors
{
    public static readonly Error NoOpenCashRegister = new(ErrorCode.NotFound, "No open cash register found for this branch.");
    public static readonly Error ProductsNotFound = new(ErrorCode.NotFound, "One or more products do not exist.");
    public static readonly Error InsufficientStock = new(ErrorCode.InvalidState, "Insufficient stock for one or more products.");
    public static readonly Error SaleCreationFailed = new(ErrorCode.ValidationError, "Sale could not be created.");
}
