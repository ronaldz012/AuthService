namespace Common.permissions;

public class PermissionsDto
{
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public List<RoleDto> Roles { get; set; } = [];
    public List<FeaturePermissionsDeductedDto> Features { get; set; } = [];
}


public class FeaturePermissionsDeductedDto
{
    public int Id { get; set; }
    public bool CanCreate { get; set; } = false;
    public bool CanRead { get; set; } = false;
    public bool CanUpdate { get; set; } = false;
    public bool CanDelete { get; set; } = false;
}