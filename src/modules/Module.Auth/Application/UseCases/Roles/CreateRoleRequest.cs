namespace Module.Auth.Application.UseCases.Roles;

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IEnumerable<RoleFeaturePermissionDto> RoleModulePermissions { get; set; } = Enumerable.Empty<RoleFeaturePermissionDto>();
    
}

public class RoleFeaturePermissionDto
{
    public int FeatureId { get; set; }
    public List<string> Permissions { get; set; } = new List<string>();
}
