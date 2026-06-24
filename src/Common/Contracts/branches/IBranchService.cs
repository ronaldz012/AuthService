using Common.Utilities;

namespace module.Auth.interfaces;

public interface IBranchService
{
    Task<Result<List<BranchResponse>>> GetBranchesByIds(List<Guid> ids);
    Task<Result<List<BranchResponse>>> GetAllBranches();

    Task<Result<bool>>  CreateBranch(CreateBranchDto request);
}