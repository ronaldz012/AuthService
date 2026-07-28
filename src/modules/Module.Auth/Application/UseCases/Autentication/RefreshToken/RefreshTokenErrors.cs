using Common.Utilities;

namespace Module.Auth.Application.UseCases.Autentication.RefreshToken;

public static class RefreshTokenErrors
{
    public static readonly Error InvalidOrExpired = new(ErrorCode.Unauthorized, "Invalid or expired refresh token");
    public static readonly Error InactiveUser = new(ErrorCode.Unauthorized, "User account is inactive");
}
