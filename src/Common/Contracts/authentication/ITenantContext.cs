namespace Common.Contracts.authentication;

public interface ITenantContext
{
    string? Schema { get; set; }

    Guid? TenantId { get; set; }
    public string? DatabaseName { get; set; }
}

public class DesignTimeTenantContext : ITenantContext
{
    public string? Schema { get; set; }
    public Guid? TenantId { get; set; }
    public string? DatabaseName { get; set; }
}
