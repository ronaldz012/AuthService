using Module.Sales.Application.UseCases.Registers.Close;
using Module.Sales.Application.UseCases.Registers.Current;
using Module.Sales.Application.UseCases.Registers.GetById;
using Module.Sales.Application.UseCases.Registers.LastClosure;
using Module.Sales.Application.UseCases.Registers.List;
using Module.Sales.Application.UseCases.Registers.Open;
using Module.Sales.Application.UseCases.Registers.TodaySales;

namespace Module.Sales.Application.UseCases;

public record RegisterUseCases(OpenCashRegister OpenCashRegister, CloseCashRegister CloseCashRegister, GetCurrentRegister GetCurrentRegister, ListClosures ListClosures, GetClosureDetail GetClosureDetail, GetTodaySales GetTodaySales, GetLastClosureSummary GetLastClosureSummary);
