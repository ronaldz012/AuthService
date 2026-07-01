using Common.Domain;

namespace Module.Auth.Domain;

public class Role :  IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Public { get; set; } = false;

        //Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }
        public int CreatedBy { get; set; }  
        public int? DeletedBy { get; set; }

    //Navigation property
    public ICollection<UserBranchRole> UserRoles { get; set; } = default!;
    public ICollection<RoleFeaturePermission> RoleFeaturePermissions { get; set; } = default!;

    public static Role CreateFromTemplate(DefaultRoleTemplate template)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            Name = template.Name,
            Description = template.Description,
            CreatedAt = DateTime.UtcNow,
            RoleFeaturePermissions = template.Permissions.Select(permTemplate => new RoleFeaturePermission
            {
                FeatureKey = permTemplate.FeatureKey,
                Permissions = permTemplate.Actions,
                CreatedAt = DateTime.UtcNow
            }).ToList()
        };
    }
}
