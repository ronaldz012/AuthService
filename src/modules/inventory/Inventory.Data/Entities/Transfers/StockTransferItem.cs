using Common.Domain;
using Inventory.Data.Entities.Products;

namespace Inventory.Data.Entities.Transfers;

public class StockTransferItem: IMustHaveTenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid TransferId { get; set; }
    public Guid ProductVariantId { get; set; }
    public int QuantityRequested { get; set; }

    public StockTransfer StockTransfer { get; set; } = null!;
    public ProductVariant ProductVariant { get; set; } = null!;
}