using Branches.Contracts.Dtos;
using Common.Result;

namespace Branches.Contracts;

public interface IBranchService
{
    Task<Result<List<BranchDto>>> GetBranchesByIds(List<Guid> ids);
    Task<Result<List<BranchDto>>> GetBranches();
    Task<Result<bool>>  CreateBranch(CreateBranchDto request);
}