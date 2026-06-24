using Common.Utilities;

namespace Module.Auth.Application.UseCases.Users.GetAllUsers;

public class UserQueryDto :GenericPaginationQueryDto
{
    public string? Email { get; set; }
}