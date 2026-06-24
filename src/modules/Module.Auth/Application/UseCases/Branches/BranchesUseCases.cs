using Module.Auth.Application.UseCases.Branches.CreateBranch;
using Module.Auth.Application.UseCases.Branches.GetBranches;
namespace Module.Auth.Application.UseCases.Branches;

public record BranchesUseCases(CreateBranch.CreateBranch CreateBranch, GetBranches.GetBranches ListBranches);