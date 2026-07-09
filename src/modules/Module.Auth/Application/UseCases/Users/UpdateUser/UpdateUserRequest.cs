using System.ComponentModel.DataAnnotations;

namespace Module.Auth.Application.UseCases.Users.UpdateUser;

public class UpdateUserRequest
{
    [MinLength(3)]
    public string? FirstName { get; set; }

    [MinLength(3)]
    public string? LastName { get; set; }

    public string? Ci { get; set; }

    public string? Nationality { get; set; }

    public DateTime? BirthDate { get; set; }

    public IEnumerable<BranchRoleDto>? BranchRoles { get; set; }
}

public class BranchRoleDto
{
    [Required]
    public Guid BranchId { get; set; }

    [Required]
    public Guid RoleId { get; set; }
}
