using Common.Extensions;

namespace Inventory.Contracts.Dtos;

public class ColorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
}

public class ColoreQueryDto : GenericPaginationQueryDto
{
    
}