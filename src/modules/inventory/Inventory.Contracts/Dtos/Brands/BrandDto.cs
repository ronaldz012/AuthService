using Common.Extensions;

namespace Inventory.Contracts.Dtos.Brands;

public class BrandDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class QueryBrandDto : GenericPaginationQueryDto
{

}