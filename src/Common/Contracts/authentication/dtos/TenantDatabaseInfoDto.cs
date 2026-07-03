namespace Common.Contracts.authentication.dtos;

public class TenantDatabaseInfoDto
{
    public required string Schema { get; set; }
    public string? DatabaseName { get; set; }
}
