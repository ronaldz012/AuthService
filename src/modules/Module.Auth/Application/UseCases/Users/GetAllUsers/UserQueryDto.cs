using Common.Utilities;

namespace Module.Auth.Application.UseCases.Users.GetAllUsers;

public class UserQueryDto : GenericPaginationQueryDto
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
}