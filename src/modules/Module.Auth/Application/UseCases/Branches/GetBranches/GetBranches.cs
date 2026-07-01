using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;

namespace Module.Auth.Application.UseCases.Branches.GetBranches;

public class GetBranches(IAuthDbContext context)
{
    public async Task<Result<List<BranchCreatedResponse>>> Execute()
    {
        return await context.Branches.Select(x => new BranchCreatedResponse()
        {
            Id = x.Id,
            Name = x.Name,
        }).ToListAsync();
    }
}