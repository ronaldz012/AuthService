using Module.Sales.Domain;

namespace Module.Sales.Application.UseCases.Sales.Return;

public record CreateReturnResponse(
    Guid ReturnSaleId,
    string ReturnNumber,
    decimal TotalRefundAmount);