using Common.permissions;

namespace Common.Contracts.authentication.dtos;

public class PermissionsDto
{
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public List<RoleDto> Roles { get; set; } = [];
    public List<FeaturePermissionsDeductedDto> Features { get; set; } = [];
}


public class FeaturePermissionsDeductedDto
{
    public string Key { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = [];
}