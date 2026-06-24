using Common.Utilities;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Branches.CreateBranch;

public class CreateBranch(IAuthDbContext context)
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