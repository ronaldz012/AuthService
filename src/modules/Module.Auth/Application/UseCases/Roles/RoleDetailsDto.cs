namespace Module.Auth.Application.UseCases.Roles;

public class RoleDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IEnumerable<FeaturePermissionsDto> FeaturePermissions { get; set; } = Enumerable.Empty<FeaturePermissionsDto>();
    
}

public class FeaturePermissionsDto
{
    public int FeatureId { get; set; }
    public List<string> Permissions { get; set; } = new List<string>();
}
