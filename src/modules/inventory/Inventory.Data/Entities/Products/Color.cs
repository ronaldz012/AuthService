using Common.Domain;
using Inventory.Data.Entities.Shared.Base;

namespace Inventory.Data.Entities.Products;

public class Color: Params,IMustHaveTenant
{
    public Guid Id { get; set; }
    public string Name { get; set; }   // "Azul Marino"
    public string Code { get; set; }   // "AZM" — único por tenant
    public Guid TenantId { get; set; }
    public ICollection<ProductVariant> ProductVariant { get; set; } = null!;
}