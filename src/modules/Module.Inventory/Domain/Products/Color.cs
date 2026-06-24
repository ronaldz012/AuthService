using Common.Domain;
using Module.Inventory.Entities.Shared.Base;

namespace Module.Inventory.Entities.Products;

public class Color: Params,IMustHaveTenant
{
    public Guid Id { get; set; }
    public required string Name { get; set; }   // "Azul Marino"
    public Guid TenantId { get; set; }
    public ICollection<ProductVariant> ProductVariant { get; set; } = null!;
}