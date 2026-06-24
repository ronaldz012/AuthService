using Common.Contracts.branches.dtos;
using Common.Utilities;

namespace Common.Contracts.branches;

public interface IBranchService
{
    Task<Result<List<BranchDto>>> GetBranchesByIds(List<Guid> ids);
    Task<Result<List<BranchDto>>> GetAllBranches();
}