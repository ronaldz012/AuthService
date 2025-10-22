using System;
using Shared.Extensions;

namespace Auth.Dtos.Modules;

public class ModuleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IEnumerable<MenuDto> Menus { get; set; } = Enumerable.Empty<MenuDto>();
}

public class ModuleQueryDto : GenericPaginationQueryDto
{
    
}