namespace Module.Inventory.Application.UseCases.Receptions.Revert;

public class StockReceptionRevertCheckDto
{
    public Guid ReceptionId { get; set; }
    public bool CanRevert { get; set; }
    public string Reason { get; set; } = string.Empty;
}