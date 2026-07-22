using Common.Utilities;

namespace Module.Auth.Application.UseCases.Users.GetAllUsers;

public class UserQueryDto : PaginationQueryDto
{
    public bool? IsActive { get; set; }
    public string? Filter { get; set; }
}