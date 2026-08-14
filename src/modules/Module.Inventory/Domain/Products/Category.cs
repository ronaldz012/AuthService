using Common.Domain;
using Module.Inventory.Domain.Shared.Base;

namespace Module.Inventory.Domain.Products;

public class Category : Params, IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = [];

    public static Category Create(string name, Guid tenantId, Guid createdBy, string createdByName)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            TenantId = tenantId,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedByName = createdByName
        };
    }

    public void Update(string name, string description, Guid updatedBy, string updatedByName)
    {
        Name = name;
        Description = description;
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
