namespace Module.Auth.Domain;

public class Tenant
{
    public Guid Id { get; set; }
    public string Schema { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? DatabaseName { get; set; }
    public string? ConnectionString { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid OwnerId { get; set; }
 
    public User OwnerUser { get; set; } = default!;
}

