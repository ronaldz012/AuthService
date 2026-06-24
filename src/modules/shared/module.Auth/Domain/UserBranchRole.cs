namespace module.Auth.Entities;
public class UserBranchRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Guid BranchId { get; set; }  // in other module
    public Guid TenantId { get; set; }

    //Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedBy { get; set; }
    public int? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation property
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}
