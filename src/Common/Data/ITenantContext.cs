namespace Common.Data;

public interface ITenantContext
{
    string? Schema { get; set; }
    bool IsDesignTime { get; }
}

// Implementación para runtime (inyectada via DI)
public class TenantContext : ITenantContext
{
    public string? Schema { get; set; }
    public bool IsDesignTime => false;
}

// Implementación para design time (usada solo en la factory)
public class DesignTimeTenantContext : ITenantContext
{
    public string? Schema { get; set; } = null;
    public bool IsDesignTime => true;
}
