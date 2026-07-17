using Module.Sales.Domain;

namespace Module.Sales.Application.UseCases.Movements.List;

public class MovementListDto
{
    public Guid Id { get; set; }
    public Guid CashRegisterClosureId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
