namespace Module.Auth.Application.UseCases.Roles.GetById;

public class RoleDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IEnumerable<FeaturePermissionsDto> FeaturePermissions { get; set; } = Enumerable.Empty<FeaturePermissionsDto>();
    
}

public class FeaturePermissionsDto
{
    public string FeatureKey { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new List<string>();
}
