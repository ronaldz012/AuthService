using Auth.Contracts.Dtos.Roles;

namespace Auth.Contracts.Dtos.permissions;

public class PermissionsDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public List<RoleDto> Roles { get; set; } = [];
    public List<FeaturePermissionsDeductedDto> Features { get; set; } = [];
}


public class FeaturePermissionsDeductedDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public int ModuleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public bool CanCreate { get; set; } = false;
    public bool CanRead { get; set; } = false;
    public bool CanUpdate { get; set; } = false;
    public bool CanDelete { get; set; } = false;
}