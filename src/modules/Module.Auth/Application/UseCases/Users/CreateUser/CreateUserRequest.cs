using System.ComponentModel.DataAnnotations;

namespace Module.Auth.Application.UseCases.Users.CreateUser;

public class CreateUserRequest
{
    [Required, MinLength(3), MaxLength(50), RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Username must be letters and numbers only")]
    public string Username { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }
    [Required, MinLength(3)]
    public string FirstName { get; set; } = string.Empty;
    [Required, MinLength(3)]
    public string LastName { get; set; } = string.Empty;
    public string Ci { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; } = DateTime.MinValue;
    
    public IEnumerable<UserBranchRoleDto> BranchRoles { get; set; } = [];
    
}
public class UserBranchRoleDto
{
    public Guid BranchId { get; set; }
    public Guid RoleId { get; set; }
}