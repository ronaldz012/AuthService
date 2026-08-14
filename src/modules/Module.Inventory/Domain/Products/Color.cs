using Common.Domain;
using Module.Inventory.Domain.Shared.Base;

namespace Module.Inventory.Domain.Products;

public class Color: Params,IMustHaveTenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Guid TenantId { get; set; }
    public ICollection<ProductVariant> ProductVariant { get; set; } = null!;

    public static Color Create(string name, Guid tenantId, Guid createdBy)
    {
        return new Color
        {
            Id = Guid.NewGuid(),
            Name = name,
            TenantId = tenantId,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedByName = "System"
        };
    }

    public void Update(string name, Guid updatedBy, string updatedByName)
    {
        Name = name;
        UpdatedBy = updatedBy;
        UpdatedByName = updatedByName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ToggleActive(Guid updatedBy, string updatedByName)
    {
        IsActive = !IsActive;
        UpdatedBy = updatedBy;
        UpdatedByName = updatedByName;
        UpdatedAt = DateTime.UtcNow;
    }
}
