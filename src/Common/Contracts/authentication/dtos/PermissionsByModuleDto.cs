
namespace Common.Contracts.authentication.dtos;

public class PermissionsByModuleDto
{
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<PermissiónByModuleDto> Modules { get; set; } = [];
}

public class PermissiónByModuleDto
{
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public List<FeaturePermissionByModuleDto> Features {get; set; } = [];
}

public class FeaturePermissionByModuleDto
{
    public string key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string route {get; set; } = string.Empty;
    public string icon {get; set; } = string.Empty;
    public bool IsMenu { get; set; } 
    public List<string> Permission { get; set; } = [];
}
