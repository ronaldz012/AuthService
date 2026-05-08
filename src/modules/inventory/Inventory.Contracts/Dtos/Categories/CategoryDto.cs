using Common.Extensions;

namespace Inventory.Contracts.Dtos.Categories;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
public class CategoryQueryDto : GenericPaginationQueryDto
{

}