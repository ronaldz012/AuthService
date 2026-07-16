using Module.Sales.Domain;

namespace Module.Sales.Application.UseCases.Movements.Create;

public class CreateMovementDto
{
    public Guid CashRegisterClosureId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public CashRegisterMovementType Type { get; set; }
}
