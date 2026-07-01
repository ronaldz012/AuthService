
using Common.Contracts.authentication;

namespace Module.Auth.Infrastructure.Authentication;

public class TenantContext : ITenantContext
{
    public string? Schema { get; set; }
    public Guid? TenantId { get; set; }
    public string? DatabaseName { get; set; }
}
