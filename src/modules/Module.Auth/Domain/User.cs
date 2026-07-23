using System.ComponentModel.DataAnnotations;
using Common.Domain;
using Module.Auth.Domain;

namespace Module.Auth.Domain;

public class User : IMustHaveTenant, ICreatedAt, ICreatedBy, IUpdatedAt, IUpdatedBy, ISoftDelete
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
    public bool IsActive { get; set; }
    public string? GoogleId { get; set; }
    public DateTime LastActive { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public string? UpdatedByName { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    public string? DeletedByName { get; set; }

    public Guid TenantId { get; set; }

    public AuthProvider AuthProvider { get; set; }
    public string? ExternalAuthId { get; set; }

    public ICollection<EmailVerificationCode> EmailVerificationCodes { get; set; } = [];
    public ICollection<UserBranchRole> UserBranchRoles { get; set; } = [];

    public static User CreateOwner(Guid id, string email, string username, Guid createdBy, string createdByName)
    {
        return new User
        {
            Id = id,
            Email = email,
            Username = username,
            PasswordHash = string.Empty,
            Status = UserStatus.PendingPasswordSetup,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Type = UserType.Owner,
            CreatedBy = createdBy,
            CreatedByName = createdByName,
        };
    }

    public static User CreateTenantAdmin(string? email, string username, string firstName, string lastName, string ci, string nationality, DateTime birthDate, Guid createdBy, string createdByName)
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
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Type = UserType.TenantAdmin,
            CreatedBy = createdBy,
            CreatedByName = createdByName,
        };
    }

    public static User CreateStandard(string? email, string username, string firstName, string lastName, string ci, string nationality, DateTime birthDate, Guid createdBy, string createdByName)
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
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Type = UserType.Standard,
            CreatedBy = createdBy,
            CreatedByName = createdByName,
        };
    }

    public void SetPassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        Status = UserStatus.Ready;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsVerified()
    {
        Status = UserStatus.Ready;
        IsActive = true;
    }

    public void Activate(Guid updatedBy, string updatedByName)
    {
        if (IsActive)
            throw new InvalidOperationException($"User {Id} is already active.");
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        UpdatedByName = updatedByName;
    }

    public void Deactivate(Guid updatedBy, string updatedByName)
    {
        if (!IsActive)
            throw new InvalidOperationException($"User {Id} is already inactive.");
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        UpdatedByName = updatedByName;
    }

    public bool CanPromoteToAdmin()
    {
        return Type is UserType.Standard;
    }

    public void PromoteToAdmin(Guid updatedBy, string updatedByName)
    {
        if (!CanPromoteToAdmin())
            throw new InvalidOperationException($"User {Id} cannot be promoted to admin. Current type: {Type}.");
        Type = UserType.TenantAdmin;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        UpdatedByName = updatedByName;
    }

    public bool CanDemoteToStandard()
    {
        return Type is UserType.TenantAdmin && UserBranchRoles.Count != 0;
    }

    public void DemoteToStandard(Guid updatedBy, string updatedByName)
    {
        if (!CanDemoteToStandard())
            throw new InvalidOperationException($"User {Id} cannot be demoted to standard. Current type: {Type}, branch roles: {UserBranchRoles.Count}.");
        Type = UserType.Standard;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        UpdatedByName = updatedByName;
    }

    public void UpdateProfile(string? firstName, string? lastName, string? ci, string? nationality, DateTime? birthDate, Guid updatedBy, string updatedByName)
    {
        if (firstName is not null) FirstName = firstName;
        if (lastName is not null) LastName = lastName;
        if (ci is not null) Ci = ci;
        if (nationality is not null) Nationality = nationality;
        if (birthDate is not null) BirthDate = birthDate.Value;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        UpdatedByName = updatedByName;
    }
}

public enum UserStatus
{
    PendingPasswordSetup = 1,
    Ready = 2,
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