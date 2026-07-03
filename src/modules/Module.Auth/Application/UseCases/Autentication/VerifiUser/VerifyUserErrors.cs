using Common.Utilities;

namespace Module.Auth.Application.UseCases.Autentication.VerifiUser;

public static class VerifyUserErrors
{
    public static readonly Error CodeNotFound = new(ErrorCode.NotFound, "Verification Code not found");
    public static readonly Error CodeExpired = new(ErrorCode.InvalidState, "Verification code Expired");
}
