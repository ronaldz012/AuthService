using Common.Utilities;

namespace Module.Auth.Application.UseCases.Users.UpdateUserStatus;

public static class UpdateUserStatusErrors
{
    public static readonly Error UserNotFound = new(ErrorCode.NotFound, "User not found");
}
