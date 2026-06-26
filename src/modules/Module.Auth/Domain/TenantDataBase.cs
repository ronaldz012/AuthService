namespace Module.Auth.Domain;

public class TenantDataBase
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Schema { get; set;}
    public ICollection<Tenant> Tenants { get; set; } = [];
}