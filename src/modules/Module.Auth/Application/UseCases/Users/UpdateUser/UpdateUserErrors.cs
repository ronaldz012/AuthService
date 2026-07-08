using Common.Utilities;

namespace Module.Auth.Application.UseCases.Users.UpdateUser;

public static class UpdateUserErrors
{
    public static readonly Error UserNotFound = new(ErrorCode.NotFound, "User not found");
    public static readonly Error BranchesNotFound = new(ErrorCode.NotFound, "One or more branches not found");
    public static readonly Error RolesNotFound = new(ErrorCode.NotFound, "One or more roles not found");
}
