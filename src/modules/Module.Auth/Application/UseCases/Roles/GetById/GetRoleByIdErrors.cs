using Common.Utilities;

namespace Module.Auth.Application.UseCases.Roles.GetById;

public static class GetRoleByIdErrors
{
    public static readonly Error RoleNotFound = new(ErrorCode.NotFound, "Role not found");
}
