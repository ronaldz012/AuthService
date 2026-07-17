using Module.Sales.Domain;

namespace Module.Sales.Application.UseCases.Movements.Create;

public class CreateMovementDto
{
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}
