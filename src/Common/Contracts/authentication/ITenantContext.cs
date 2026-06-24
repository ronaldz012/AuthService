namespace Common.Contracts.authentication;

public interface ITenantContext
{
    string? Schema { get; set; }
    bool IsDesignTime { get; }
    Guid? TenantId { get; set; }
    public string? DatabaseName { get; set; }
}

// Implementación para runtime (inyectada via DI)
public class TenantContext : ITenantContext
{
    public string? Schema { get; set; }
    public bool IsDesignTime => false;
    public Guid? TenantId { get; set; }
    public string? DatabaseName { get; set; }
}

// Implementación para design time (usada solo en la factory)
public class DesignTimeTenantContext : ITenantContext
{
    public string? Schema { get; set; } = null;
    public bool IsDesignTime => true;
    public Guid? TenantId { get; set; }
    public string? DatabaseName { get; set; }
}
