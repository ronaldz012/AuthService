namespace System.Api.Data;

public interface ITenantContext
{
    string Schema { get; }
}
public class TenantContext : ITenantContext
{
    public string Schema { get; set; } = string.Empty;
}
public class DesignTimeTenantContext : ITenantContext
{
    public string Schema => "client1";
}