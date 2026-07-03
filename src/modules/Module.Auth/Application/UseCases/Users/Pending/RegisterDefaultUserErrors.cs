using Common.Utilities;

namespace Module.Auth.Application.UseCases.Users.Pending;

public static class RegisterDefaultUserErrors
{
    public static readonly Error NotImplemented = new(ErrorCode.InternalError, "Not implemented");
}
