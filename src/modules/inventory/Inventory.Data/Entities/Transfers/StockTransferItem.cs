using Inventory.Data.Entities.Products;

namespace Inventory.Data.Entities.Transfers;

public class StockTransferItem
{
    public Guid Id { get; set; }
    public Guid TransferId { get; set; }
    public Guid ProductVariantId { get; set; }
    public int QuantityRequested { get; set; }

    public StockTransfer StockTransfer { get; set; } = null!;
    public ProductVariant ProductVariant { get; set; } = null!;
}