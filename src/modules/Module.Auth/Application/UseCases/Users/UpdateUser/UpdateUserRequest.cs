using System.ComponentModel.DataAnnotations;

namespace Module.Auth.Application.UseCases.Users.UpdateUser;

public class UpdateUserRequest
{
    [Required, MinLength(3)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MinLength(3)]
    public string LastName { get; set; } = string.Empty;

    public string Ci { get; set; } = string.Empty;

    public string Nationality { get; set; } = string.Empty;

    public DateTime BirthDate { get; set; } = DateTime.MinValue;

    [Required, MinLength(1)]
    public IEnumerable<BranchRoleDto> BranchRoles { get; set; } = [];
}

public class BranchRoleDto
{
    [Required]
    public Guid BranchId { get; set; }

    [Required]
    public Guid RoleId { get; set; }
}
