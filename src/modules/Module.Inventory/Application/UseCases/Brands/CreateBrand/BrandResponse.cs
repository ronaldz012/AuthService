using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Brands.CreateBrand;

public class BrandResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
