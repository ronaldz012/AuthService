using Common.Utilities;

namespace Module.Auth.Application.UseCases.Branches.UpdateBranch;

public static class UpdateBranchErrors
{
    public static readonly Error BranchNotFound = new(ErrorCode.NotFound, "Branch not found");
}
