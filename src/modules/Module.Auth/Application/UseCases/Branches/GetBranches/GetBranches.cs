using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Application.UseCases.Branches.CreateBranch;

namespace Module.Auth.Application.UseCases.Branches.GetBranches;

public class GetBranches(IAuthDbContext context, ITenantContext tenantContext)
{
    public async Task<Result<List<BranchCreatedResponse>>> Execute()
    {
        Console.WriteLine($"TenantId in GetBranches: {tenantContext.TenantId}");
        return await context.Branches.Select(x => new BranchCreatedResponse()
        {
            Id = x.Id,
            Name = x.Name,
        }).ToListAsync();
    }
}