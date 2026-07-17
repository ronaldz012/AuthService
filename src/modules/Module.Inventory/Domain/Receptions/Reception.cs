using Common.Domain;
using Module.Inventory.Domain.Products;
using Module.Inventory.Domain.Shared.Base;

namespace Module.Inventory.Domain.Receptions;

public class StockReception : Params, IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public DateTime ReceivedAt { get; set; }
    public ReceptionStatus Status { get; set; } = ReceptionStatus.Confirmed;
    public string? Notes { get; set; }

    public ICollection<StockReceptionItem> Items { get; set; } = new List<StockReceptionItem>();

    public static StockReception Create(Guid branchId, string? notes)
    {
        return new StockReception
        {
            Id = Guid.NewGuid(),
            BranchId = branchId,
            Notes = notes,
            ReceivedAt = DateTime.UtcNow,
            Status = ReceptionStatus.Confirmed
        };
    }

    public void AddExistingVariant(Guid variantId, int quantity, decimal unitCost)
    {
        Items.Add(new StockReceptionItem
        {
            ProductVariantId = variantId,
            QuantityReceived = quantity,
            UnitCost = unitCost,
        });
    }
}

public class StockReceptionItem : Params, IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid StockReceptionId { get; set; }
    public Guid ProductVariantId { get; set; }
    public int QuantityReceived { get; set; }
    public decimal UnitCost { get; set; }

    public StockReception StockReception { get; set; } = null!;
    public ProductVariant ProductVariant { get; set; } = null!;
}

public enum ReceptionStatus
{
    Draft = 0,
    Confirmed = 1,
    Rejected = 2,
    Reverted = 3,
}

