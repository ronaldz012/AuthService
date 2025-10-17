using System;
using Shared.Domain;

namespace Auth.Data.Entities;

public class RoleModulePermission : ICreatedAt, IUpdatedAt, IUpdatedBy
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public int ModuleId { get; set; }
    public bool CanCreate { get; set; } = false;
    public bool CanRead { get; set; } = false;
    public bool CanUpdate { get; set; } = false;
    public bool CanDelete { get; set; } = false;

    //Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    // Navigation property
    public Role Role { get; set; } = default!;
    public Module Module { get; set; } = default!;
}
