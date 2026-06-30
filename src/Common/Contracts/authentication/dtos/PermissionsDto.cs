using Common.permissions;

namespace Common.Contracts.authentication.dtos;

public class PermissionsDto
{
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public List<FeaturePermissionsDto> Features { get; set; } = [];
}


public class FeaturePermissionsDto
{
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsMenu { get; set; } 
    public string Icon { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = [];
}