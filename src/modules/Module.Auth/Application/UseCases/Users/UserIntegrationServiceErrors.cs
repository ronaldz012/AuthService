using Common.Utilities;

namespace Module.Auth.Application.UseCases.Users;

public static class UserIntegrationServiceErrors
{
    public static readonly Error UsersNotFound = new(ErrorCode.NotFound, "Users not found");
}
