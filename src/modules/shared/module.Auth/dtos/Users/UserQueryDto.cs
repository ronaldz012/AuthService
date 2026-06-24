using Common.Extensions;

namespace shared.Contracts.dtos.Users;

public class UserQueryDto :GenericPaginationQueryDto
{
    public string? Email { get; set; }
}