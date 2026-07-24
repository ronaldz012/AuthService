using Common.Domain;
using Module.Inventory.Domain.Shared.Base;

namespace Module.Inventory.Domain.Products;

public class Color: Params,IMustHaveTenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public ICollection<ProductVariant> ProductVariant { get; set; } = null!;

    public static Color Create(string name, Guid tenantId, Guid createdBy)
    {
        return new Color
        {
            Id = Guid.NewGuid(),
            Name = name,
            TenantId = tenantId,
            CreatedBy = createdBy,
            CreatedByName = "System"
        };
    }
}
