using Common.Domain;

namespace module.Auth.Entities;

public class Role :  IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsAdmin { get; set; } = false;
    public bool Public { get; set; } = false;

        //Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }
        public int CreatedBy { get; set; }  
        public int? DeletedBy { get; set; }

    //Navigation property
    public ICollection<UserBranchRole> UserRoles { get; set; } = default!;
    public ICollection<RoleFeaturePermission> RoleFeaturePermissions { get; set; } = default!;

}
