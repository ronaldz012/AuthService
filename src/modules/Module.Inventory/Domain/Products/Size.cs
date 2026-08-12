using Common.Domain;
using Module.Inventory.Domain.Shared.Base;

namespace Module.Inventory.Domain.Products;

public class Size : Params, IMustHaveTenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public Guid TenantId { get; set; }
    public ICollection<ProductVariant> ProductVariants { get; set; } = null!;

    public static Size Create(string name, int sortOrder, Guid tenantId, Guid createdBy)
    {
        return new Size
        {
            Id = Guid.NewGuid(),
            Name = name,
            SortOrder = sortOrder,
            TenantId = tenantId,
            CreatedBy = createdBy,
            CreatedByName = "System"
        };
    }
}