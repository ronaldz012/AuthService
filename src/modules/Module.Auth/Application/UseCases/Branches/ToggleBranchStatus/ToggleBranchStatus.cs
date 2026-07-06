using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;

namespace Module.Auth.Application.UseCases.Branches.ToggleBranchStatus;

public class ToggleBranchStatus(IAuthDbContext context)
{
    public async Task<Result<bool>> Execute(Guid id)
    {
        var branch = await context.Branches.FirstOrDefaultAsync(b => b.Id == id);
        if (branch == null) return ToggleBranchStatusErrors.BranchNotFound;

        branch.IsActive = !branch.IsActive;
        await context.SaveChangesAsync();
        return true;
    }
}
