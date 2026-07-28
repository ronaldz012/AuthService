using Common.Utilities;

namespace Module.Auth.Application.UseCases.Autentication.Login;

public static class LoginErrors
{
    public static readonly Error UserNotFound = new(ErrorCode.NotFound, "User not found");
    public static readonly Error InvalidPassword = new(ErrorCode.ValidationError, "Invalid password");
    public static readonly Error InactiveUser = new(ErrorCode.Unauthorized, "User account is inactive");
}
