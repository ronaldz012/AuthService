using Auth.Contracts.Dtos.Roles;

namespace Auth.Contracts.Dtos.permissions;

public class PermissionsByModuleDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public List<RoleDto> Roles { get; set; } = [];
    public List<PermissiónByModuleDto> Modules { get; set; } = [];
}

public class PermissiónByModuleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public List<FeaturePermissionByModuleDto> Features {get; set; } = [];
}

public class FeaturePermissionByModuleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool CanCreate { get; set; } = false;
    public bool CanRead { get; set; } = false;
    public bool CanUpdate { get; set; } = false;
    public bool CanDelete { get; set; } = false;
}