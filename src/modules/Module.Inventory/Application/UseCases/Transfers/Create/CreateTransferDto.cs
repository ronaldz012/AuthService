namespace Module.Inventory.Application.UseCases.Transfers.Create;

public class CreateStockTransferDto
{
    public Guid ToBranchId { get; set; }
    public string? Notes { get; set; }
    public List<StockTransferItemDto> Items { get; set; } = new();
}

public class StockTransferItemDto
{
    public Guid ProductVariantId { get; set; }
    public int QuantityRequested { get; set; }
}