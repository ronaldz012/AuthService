using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Colors.List;

public class ListColorResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ColoreQueryDto : GenericPaginationQueryDto
{
    
}