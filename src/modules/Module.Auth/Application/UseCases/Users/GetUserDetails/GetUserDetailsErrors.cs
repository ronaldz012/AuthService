using Common.Utilities;

namespace Module.Auth.Application.UseCases.Users.GetUserDetails;

public static class GetUserDetailsErrors
{
    public static readonly Error UserNotFound = new(ErrorCode.NotFound, "User not found");
}
