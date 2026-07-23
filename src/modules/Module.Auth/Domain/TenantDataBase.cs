using Common.Domain;

namespace Module.Auth.Domain;

public class TenantDataBase : ICreatedAt, ICreatedBy
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Schema { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;

    public ICollection<Tenant> Tenants { get; set; } = [];
}