using Module.Sales.Application.UseCases.Sales.Create;
using Module.Sales.Application.UseCases.Sales.Get;
using Module.Sales.Application.UseCases.Sales.GetById;

namespace Module.Sales.Application.UseCases;

public record SaleUseCases(CreateSale CreateSale, GetListSales GetListSales, GetSaleDetail GetSaleDetail);