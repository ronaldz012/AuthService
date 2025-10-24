using System.ComponentModel.DataAnnotations;
using Shared.Domain;
namespace Auth.Data.Entities;

public class User : ICreatedAt, ICreatedBy, IUpdatedAt, ISoftDelete, IUpdatedBy
{
    [Key]
    public int Id { get; set; }
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;

    public byte[] PasswordHash { get; set; } = default!;
    public byte[] PasswordSalt { get; set; } = default!;
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    public bool IsEmailConfirmed { get; set; } = false;
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    [StringLength(100)]
    public string FatherLastName { get; set; } = string.Empty;
    [StringLength(100)]
    public string MotherLastName { get; set; } = string.Empty;
    [StringLength(15)]
    public string Ci { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; } = DateTime.MinValue;
    public bool Status { get; set; } = true;
    public string GoogleId { get; set; } = string.Empty;
    public bool IsGoogleAuthenticated { get; set; } = false;
    public DateTime LastActive { get; set; }
    //Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public int? DeletedBy { get; set; }
    public int CreatedBy { get; set; }

    // Navigation property
    public ICollection<UserRole> UserRoles { get; set; } = default!;
}

public enum UserStatus
{
    PendingVerification = 0,
    Active = 1,
    Suspended = 2,
    Inactive = 3
}

public enum AuthProvider
{
    Local = 0,
    Google = 1,
    Facebook = 2,
    Microsoft = 3
}   