using Common.Utilities;

namespace Module.Auth.Application.UseCases.Autentication.SetupUserPassword;

public static class SetupUserPasswordErrors
{
    public static readonly Error TokenNotFound = new(ErrorCode.NotFound, "The verification token is invalid or has already been used.");
    public static readonly Error TokenExpired = new(ErrorCode.ValidationError, "The verification token has expired.");
}
