using Common.Domain;

namespace Module.Auth.Domain;

public class Plan : ICreatedAt, ICreatedBy
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int MaxUsers { get; set; }
    public int MaxBranches { get; set; }
    public int MaxExtraRoles { get; set; }
    public List<string> AllowedFeatureKeys { get; set; } = new List<string>();
    public List<DefaultRoleTemplate> DefaultRolesTemplate { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
}

public class DefaultRoleTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<DefaultRolePermissionTemplate> Permissions { get; set; } = new();
}

public class DefaultRolePermissionTemplate
{
    public string FeatureKey { get; set; } = string.Empty;
    public List<string> Actions { get; set; } = new();
}