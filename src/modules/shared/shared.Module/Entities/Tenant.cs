namespace shared.Module.Entities;

public class Tenant
{
    public string Id { get; set; } = string.Empty;       // "client1"
    public string DisplayName { get; set; } = string.Empty;
    public string? DatabaseName { get; set; }             // para Paso 3
    public string? ConnectionString { get; set; }         // para Paso 3
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}