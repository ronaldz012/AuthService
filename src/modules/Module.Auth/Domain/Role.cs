using Common.Domain;

namespace Module.Auth.Domain;

public class Role : IMustHaveTenant, ICreatedAt, ICreatedBy, ISoftDelete
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Public { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    public string? DeletedByName { get; set; }

    public ICollection<UserBranchRole> UserRoles { get; set; } = default!;
    public ICollection<RoleFeaturePermission> RoleFeaturePermissions { get; set; } = default!;

    public static Role CreateFromTemplate(DefaultRoleTemplate template, Guid createdBy, string createdByName)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            Name = template.Name,
            Description = template.Description,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            CreatedByName = createdByName,
            RoleFeaturePermissions = template.Permissions.Select(permTemplate => new RoleFeaturePermission
            {
                FeatureKey = permTemplate.FeatureKey,
                Permissions = permTemplate.Actions,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                CreatedByName = createdByName,
            }).ToList()
        };
    }
}
