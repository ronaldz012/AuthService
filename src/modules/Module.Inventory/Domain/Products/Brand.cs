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
    public Guid TenantId { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}