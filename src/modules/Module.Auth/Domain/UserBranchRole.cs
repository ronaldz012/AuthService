using Common.Domain;

namespace Module.Auth.Domain;

public class UserBranchRole : IMustHaveTenant, ICreatedAt, ICreatedBy, ISoftDelete
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Guid BranchId { get; set; }
    public Guid TenantId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    public string? DeletedByName { get; set; }

    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
    public Branch Branch { get; set; } = null!;

    public static UserBranchRole Create(Guid userId, Guid branchId, Guid roleId, Guid createdBy, string createdByName)
    {
        return new UserBranchRole
        {
            UserId = userId,
            BranchId = branchId,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            CreatedByName = createdByName,
        };
    }
}
