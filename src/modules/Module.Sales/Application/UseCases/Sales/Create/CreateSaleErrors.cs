using Common.Utilities;

namespace Module.Sales.Application.UseCases.Sales.Create;

public static class CreateSaleErrors
{
    public static readonly Error CashClosureNotFound = new(ErrorCode.NotFound, "Cash closure not found or does not belong to this branch.");
    public static readonly Error CashClosureNotOpen = new(ErrorCode.ValidationError, "The cash closure must be open to register sales.");
    public static readonly Error ProductsNotFound = new(ErrorCode.NotFound, "One or more products do not exist.");
    public static readonly Error SaleCreationFailed = new(ErrorCode.ValidationError, "Sale could not be created.");
}
