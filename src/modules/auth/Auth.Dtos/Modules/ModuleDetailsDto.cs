using System;

namespace Auth.Dtos.Modules;

public class ModuleDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public IEnumerable<MenuDto> Menus { get; set; } = Array.Empty<MenuDto>();
}
