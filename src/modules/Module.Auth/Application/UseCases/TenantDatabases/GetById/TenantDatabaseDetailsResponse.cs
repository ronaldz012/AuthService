namespace Module.Auth.Application.UseCases.TenantDatabases.GetById;

public class TenantDatabaseDetailsResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Schema { get; set;}
    public bool IsOnline { get; set; }

    public IEnumerable<TenantDatabaseCompanyDetailsResponse> Tenants { get; set; } = [];
    
}
public class TenantDatabaseCompanyDetailsResponse
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string PlaneName { get; set; } = string.Empty;
}