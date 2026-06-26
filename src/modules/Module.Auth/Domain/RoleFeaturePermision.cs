using Common.Domain;
using Module.Auth.Domain;

namespace Module.Auth.Domain;

public class RoleFeaturePermission : IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public string FeatureKey { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new List<string>();
    public Guid TenantId { get; set; }

    //Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    // Navigation property
    public Role Role { get; set; } = default!;
    public Feature Feature { get; set; } = null!;

}
