using System;
using Common.Domain;

namespace Auth.Data.Entities;

public class RoleFeaturePermission : ICreatedAt, IUpdatedAt, IUpdatedBy, IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public int FeatureId { get; set; }
    public bool CanCreate { get; set; } = false;
    public bool CanRead { get; set; } = false;
    public bool CanUpdate { get; set; } = false;
    public bool CanDelete { get; set; } = false;
    public Guid TenantId { get; set; }

    //Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    // Navigation property
    public Role Role { get; set; } = default!;

}
