namespace Module.Sales.Application.UseCases.Movements.Update;

public class UpdateMovementDto
{
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}
