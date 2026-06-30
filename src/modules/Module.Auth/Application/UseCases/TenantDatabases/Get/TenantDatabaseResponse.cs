namespace Module.Auth.Application.UseCases.TenantDatabases.Get;

public class TenantDatabaseResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Schema { get; set;}
    public required string Description { get; set; }
}