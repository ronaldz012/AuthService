using Common.Domain;
using Module.Inventory.Domain.Shared.Base;

namespace Module.Inventory.Domain.Products;

public class Brand : Params, IMustHaveTenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ProductCounter { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid TenantId { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();

    public static Brand Create(string name, string prefix, Guid tenantId, Guid createdBy, string createdByName)
    {
        return new Brand
        {
            Id = Guid.NewGuid(),
            Name = name,
            Prefix = prefix,
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
