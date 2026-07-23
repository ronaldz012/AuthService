using Common.Domain;
using Module.Auth.Domain;

namespace Module.Auth.Domain;

public class RoleFeaturePermission : IMustHaveTenant, ICreatedAt, ICreatedBy, IUpdatedAt, IUpdatedBy
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public string FeatureKey { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new List<string>();
    public Guid TenantId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public string? UpdatedByName { get; set; }

    public Role Role { get; set; } = default!;
    public Feature Feature { get; set; } = null!;
}
