using System.ComponentModel.DataAnnotations;
using Common.Domain;
using Module.Auth.Domain;

namespace Module.Auth.Domain;

public class User : IMustHaveTenant
{
    [Key]
    public Guid Id { get; set; }
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;
    public UserType Type { get; set; } = UserType.Standard;
    public bool IsAdmin => Type is UserType.TenantAdmin or UserType.Owner;

    public string PasswordHash { get; set; } = string.Empty;
    [StringLength(100)]
    public string? Email { get; set; } = string.Empty;
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    [StringLength(15)]
    public string Ci { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; } = DateTime.MinValue;
    public UserStatus Status { get; set; } 
    public string? GoogleId { get; set; } 
    public DateTime LastActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public int? DeletedBy { get; set; }
    public int CreatedBy { get; set; }
    
    public Guid TenantId { get; set; }

    public AuthProvider AuthProvider { get; set; }
    public string? ExternalAuthId { get; set; } 

    // Navigation property
    public ICollection<EmailVerificationCode> EmailVerificationCodes { get; set; } = [];
    public ICollection<UserBranchRole> UserBranchRoles { get; set; } = [];

    public static User CreateOwner(Guid id, string email, string username)
    {
        return new User
        {
            Id = id,
            Email = email,
            Username = username,
            PasswordHash = string.Empty,
            Status = UserStatus.PendingPasswordSetup,
            CreatedAt = DateTime.UtcNow,
            Type = UserType.Owner,
        };
    }

    public static User CreateStandard(string? email, string username, string firstName, string lastName, string ci, string nationality, DateTime birthDate)
    {
        return new User
        {
            Email = email,
            Username = username,
            FirstName = firstName,
            LastName = lastName,
            Ci = ci,
            Nationality = nationality,
            BirthDate = birthDate,
            PasswordHash = string.Empty,
            Status = UserStatus.PendingPasswordSetup,
            CreatedAt = DateTime.UtcNow,
            Type = UserType.Standard,
        };
    }
}

public enum UserStatus
{
    Active = 1,
    InActive = 2,
    PendingPasswordSetup = 3,
}

public enum AuthProvider
{
    Local = 0,
    Google = 1,
    Facebook = 2,
    Microsoft = 3
}   
public enum UserType
{
    Standard = 0,     
    TenantAdmin = 1,  
    Owner = 2
}