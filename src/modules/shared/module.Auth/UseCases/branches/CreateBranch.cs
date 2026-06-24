using Common.Utilities;
using module.Auth.Domain;

namespace module.Auth.Features.branches;

public class CreateBranch(AuthDbContext context)
{
    public async Task<Result<BranchResponse>> Execute(CreateBranchRequest request)
    {
        var newBranch = new Branch
        {
            Name = request.Name,
            Place = request.Place,
            PhoneNumber = request.PhoneNumber,
            BranchCode = request.BranchCode,
        };
        context.Branches.Add(newBranch);
        await context.SaveChangesAsync();
        return new BranchResponse{
            
            Id = newBranch.Id,
            Name = newBranch.Name,
        };
    }
}