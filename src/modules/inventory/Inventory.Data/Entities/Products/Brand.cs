using Common.Domain;
using Inventory.Data.Entities.Shared.Base;

namespace Inventory.Data.Entities.Products;

public class Brand : Params, IMustHaveTenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}