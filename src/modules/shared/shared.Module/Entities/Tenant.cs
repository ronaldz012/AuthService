namespace shared.Module.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Schema {get; set;} = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? DatabaseName { get; set; }             // para Paso 3
    public string? ConnectionString { get; set; }         // para Paso 3
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}