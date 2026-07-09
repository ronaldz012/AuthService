using Common.Utilities;

namespace Module.Auth.Application.UseCases.Users.CreateTenantAdmin;

public static class CreateTenantAdminErrors
{
    public static readonly Error EmailOrUsernameTaken = new(ErrorCode.Conflict, "Email or username already taken");
}
