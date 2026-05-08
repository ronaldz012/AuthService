using Common.Domain;
using Inventory.Data.Entities.Products;
using Inventory.Data.Entities.Shared.Base;

namespace Inventory.Data.Entities.Inventory;

public class BranchInventory : Params, IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; } //External ID
    public Guid ProductVariantId { get; set; }
    public int Stock { get; set; }
    public int MinStock { get; set; }

    public ProductVariant ProductVariant { get; set; } = null!;
    public bool ValidateQuantity(int quantity)
    {
        return Stock >= quantity;
    }
}