using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Users.GetUserDetails;

public class GetUserDetailsResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Ci { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public UserType UserType { get; set; }
    public bool IsAdmin { get; set; }
    public UserStatus Status { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<UserBranchRoleDetailDto> BranchRoles { get; set; } = [];
}

public class UserBranchRoleDetailDto
{
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}
