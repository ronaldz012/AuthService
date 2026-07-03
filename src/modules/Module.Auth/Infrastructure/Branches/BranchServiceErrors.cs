using Common.Utilities;

namespace Module.Auth.Infrastructure.Branches;

public static class BranchServiceErrors
{
    public static readonly Error BranchesNotFound = new(ErrorCode.NotFound, "One or more branches not found");
}
