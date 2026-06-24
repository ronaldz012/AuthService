using Common.Domain;
using Module.Inventory.Domain.Shared.Base;

namespace Module.Inventory.Domain.Products;

public class Category : Params, IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = [];
}
