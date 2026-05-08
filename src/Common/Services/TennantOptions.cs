namespace Common.Services;

public class TenantOptions
{
    public const string Section = "Tenants";
    public List<string> Schemas { get; set; } = [];
}