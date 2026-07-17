namespace Common.Contracts.authentication.dtos;

public class TenantDatabaseInfoDto
{
    public required string Schema { get; set; }
    public string? DatabaseName { get; set; }
    public Guid TenantId { get; set; }
    public Guid MainBranchId { get; set; }
    public Guid OwnerUserId { get; set; }
}
