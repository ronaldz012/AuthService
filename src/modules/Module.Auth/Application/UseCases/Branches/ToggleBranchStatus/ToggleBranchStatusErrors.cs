using Common.Utilities;

namespace Module.Auth.Application.UseCases.Branches.ToggleBranchStatus;

public static class ToggleBranchStatusErrors
{
    public static readonly Error BranchNotFound = new(ErrorCode.NotFound, "Branch not found");
    public static readonly Error BranchHasOpenClosures = new(ErrorCode.BadRequest, "The branch has pending operations");
}
