using Common.Utilities;

namespace Module.Auth.Application.UseCases.Branches.CreateBranch;

public static class CreateBranchErrors
{
    public static readonly Error PlanNotFound = new(ErrorCode.NotFound, "Plan not found for the current tenant");
}