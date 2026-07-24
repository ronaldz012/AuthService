using Common.Domain;
using Module.Inventory.Domain.Products;
using Module.Inventory.Domain.Shared.Base;

namespace Module.Inventory.Domain.Transfers;

public class StockTransferItem : Params, IMustHaveTenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid TransferId { get; set; }
    public Guid ProductVariantId { get; set; }
    public int QuantityRequested { get; set; }

    public StockTransfer StockTransfer { get; set; } = null!;
    public ProductVariant ProductVariant { get; set; } = null!;
}