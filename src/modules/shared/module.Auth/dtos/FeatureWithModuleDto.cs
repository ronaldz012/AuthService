namespace shared.Contracts.dtos;

public class FeatureWithModuleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int ModuleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleRoute { get; set; } = string.Empty;
    public string ModuleIcon { get; set; } = string.Empty;
    public bool ModuleIsEnabled { get; set; }
    public string ModuleDescription { get; set; } = string.Empty;  // ← agregar
}