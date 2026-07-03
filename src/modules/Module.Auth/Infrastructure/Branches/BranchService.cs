using Common.Contracts.branches;
using Common.Contracts.branches.dtos;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Application.UseCases.Branches.CreateBranch;
using Module.Auth.Domain;

namespace Module.Auth.Infrastructure.Branches;

public class BranchService(IAuthDbContext context) : IBranchService
{
    public async Task<Result<List<BranchDto>>> GetBranchesByIds(List<Guid> ids)
    {
        var branches = await context.Branches
            .Where(b => ids.Contains(b.Id))
            .Select(b => new BranchDto
            {
                Id = b.Id,
                Name = b.Name,
                Status = b.Status,
            }).ToListAsync();

        var foundIds = branches.Select(b => b.Id).ToList();
        var missingIds = ids.Except(foundIds).ToList();

        if (missingIds.Any())
            return BranchServiceErrors.BranchesNotFound;

        return branches;
    }

    public async Task<Result<List<BranchDto>>> GetAllBranches()
    {
        return await context.Branches.AsNoTracking().Select( x => new BranchDto()
        {
            Id = x.Id,
            Name = x.Name,
            BranchCode =  x.BranchCode,
            Status =  x.Status,
        }).ToListAsync();
    }
    


    public async Task<Result<bool>> CreateBranch(CreateBranchRequest request)
    {
        var newBranch = new Branch
        {
            Name = request.Name,
            Place = request.Place,
            PhoneNumber = request.PhoneNumber,
            Status = true,
            BranchCode = request.BranchCode,
        };
        await context.Branches.AddAsync(newBranch);
        await context.SaveChangesAsync();
        return true;
    }
}