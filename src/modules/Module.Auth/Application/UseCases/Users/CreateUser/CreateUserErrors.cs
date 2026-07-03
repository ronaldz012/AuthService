using Common.Utilities;

namespace Module.Auth.Application.UseCases.Users.CreateUser;

public static class CreateUserErrors
{
    public static readonly Error EmailOrUsernameTaken = new(ErrorCode.Conflict, "Email or username already taken");
    public static readonly Error BranchesNotFound = new(ErrorCode.NotFound, "One or more branches not found");
    public static readonly Error RolesNotFound = new(ErrorCode.NotFound, "One or more roles not found");
    public static readonly Error MissingRoles = new(ErrorCode.NotFound, "Roles not found");
}
