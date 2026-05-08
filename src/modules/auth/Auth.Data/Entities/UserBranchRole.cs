using System;
using Branches.module.Entities;
using Common.Domain;

namespace Auth.Data.Entities;
public class UserBranchRole : ICreatedBy, ICreatedAt, ISoftDelete
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Guid BranchId { get; set; }  // in other module

    //Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedBy { get; set; }
    public int? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation property
    public User User { get; set; } = default!;
    public Role Role { get; set; } = default!;
}
