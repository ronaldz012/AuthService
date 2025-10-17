using System;

namespace Auth.Dtos.Modules;

public class CreateModuleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IEnumerable<int> MenuIds { get; set; } = Enumerable.Empty<int>();
}
