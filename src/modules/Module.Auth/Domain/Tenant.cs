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

    public static Tenant Create(Guid id, string displayName, Guid databaseId, Guid planId, User ownerUser)
    {
        return new Tenant
        {
            Id = id,
            DisplayName = displayName,
            IsActive = true,
            DataBaseId = databaseId,
            PlanId = planId,
            OwnerId = ownerUser.Id,
            OwnerUser = ownerUser,
            CreatedAt = DateTime.UtcNow
        };
    }
}

