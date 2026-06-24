using Common.Domain;
using Module.Inventory.Domain.Shared.Base;

namespace Module.Inventory.Domain.Products;

public class Color: Params,IMustHaveTenant
{
    public Guid Id { get; set; }
    public required string Name { get; set; }   // "Azul Marino"
    public Guid TenantId { get; set; }
    public ICollection<ProductVariant> ProductVariant { get; set; } = null!;
}