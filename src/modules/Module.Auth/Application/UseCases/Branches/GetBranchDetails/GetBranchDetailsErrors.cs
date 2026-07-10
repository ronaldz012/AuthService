using Common.Utilities;

namespace Module.Auth.Application.UseCases.Branches.GetBranchDetails;

public static class GetBranchDetailsErrors
{
    public static readonly Error BranchNotFound = new(ErrorCode.NotFound, "Branch not found");
}
