namespace Module.Auth.Application.UseCases.Roles.Create;

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IEnumerable<RoleFeaturePermissionDto> RoleModulePermissions { get; set; } = Enumerable.Empty<RoleFeaturePermissionDto>();
    
}

public class RoleFeaturePermissionDto
{
    public string FeatureKey { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new List<string>();
}
