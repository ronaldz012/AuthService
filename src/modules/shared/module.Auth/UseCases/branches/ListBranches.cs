using Common.Utilities;
using Microsoft.EntityFrameworkCore;

namespace module.Auth.Features.branches;

public class ListBranches(AuthDbContext context)
{
    public async Task<Result<List<BranchResponse>>> Execute()
    {
        return await context.Branches.Select(x => new BranchResponse()
        {
            Id = x.Id,
            Name = x.Name,
        }).ToListAsync();
    }
}