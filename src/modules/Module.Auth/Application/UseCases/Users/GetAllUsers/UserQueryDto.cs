using Common.Utilities;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Users.GetAllUsers;

public class UserQueryDto : GenericPaginationQueryDto
{
    public bool? IsActive { get; set; }
}