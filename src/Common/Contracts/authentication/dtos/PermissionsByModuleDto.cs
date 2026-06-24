using Common.permissions;

namespace Common.Contracts.authentication.dtos;

public class PermissionsByModuleDto
{
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public List<RoleDto> Roles { get; set; } = [];
    public List<PermissiónByModuleDto> Modules { get; set; } = [];
}

public class PermissiónByModuleDto
{
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
    public string route {get; set; } = string.Empty;
    public string icon {get; set; } = string.Empty;
    public List<string> Permission { get; set; } = [];
}
