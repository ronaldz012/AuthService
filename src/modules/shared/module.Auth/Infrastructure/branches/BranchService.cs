using Common.Result;
using Microsoft.EntityFrameworkCore;
using module.Auth.dtos.branches;
using module.Auth.Entities;
using module.Auth.interfaces;

namespace module.Auth.branches;

public class BranchService(AuthDbContext context) : IBranchService
{
    public async Task<Result<List<BranchDto>>> GetBranchesByIds(List<Guid> ids)
    {
        var branches = await context.Branches
            .Where(b => ids.Contains(b.Id) && b.Status)
            .Select(b => new BranchDto
            {
                Id = b.Id,
                Name = b.Name,
                Status = b.Status,
            }).ToListAsync();

        var foundIds = branches.Select(b => b.Id).ToList();
        var missingIds = ids.Except(foundIds).ToList();

        if (missingIds.Any())
            return new Error("NOT_FOUND", $"Branches not found: {string.Join(", ", missingIds)}");

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
    


    public async Task<Result<bool>> CreateBranch(CreateBranchDto request)
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