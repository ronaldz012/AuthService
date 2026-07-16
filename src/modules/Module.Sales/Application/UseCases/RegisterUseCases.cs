using Module.Sales.Application.UseCases.Registers.Close;
using Module.Sales.Application.UseCases.Registers.Current;
using Module.Sales.Application.UseCases.Registers.Open;

namespace Module.Sales.Application.UseCases;

public record RegisterUseCases(OpenCashRegister OpenCashRegister, CloseCashRegister CloseCashRegister, GetCurrentRegister GetCurrentRegister);
