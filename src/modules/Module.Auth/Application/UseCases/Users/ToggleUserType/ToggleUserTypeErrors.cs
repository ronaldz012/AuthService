using Common.Utilities;

namespace Module.Auth.Application.UseCases.Users.ToggleUserType;

public static class ToggleUserTypeErrors
{
    public static readonly Error UserNotFound = new(ErrorCode.NotFound, "User not found");
    public static readonly Error CannotToggleOwner = new(ErrorCode.BadRequest, "Cannot change owner user type");
    public static readonly Error NoBranchRolesAssigned = new(ErrorCode.Conflict, "User must have at least one branch role assigned before downgrading to Standard");
}
