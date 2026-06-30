namespace Module.Auth.Domain;

public class Tenant
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid OwnerId { get; set; }
    public Guid DataBaseId { get; set; }
    public Guid PlanId { get; set; }

    public Plan Plan { get; set; } = null!;
 
    public User OwnerUser { get; set; } = null!;
    public TenantDataBase TenantDataBase { get; set; } = null!;
    
    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
}

