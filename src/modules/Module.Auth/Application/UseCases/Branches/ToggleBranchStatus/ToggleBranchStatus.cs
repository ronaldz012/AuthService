using Common.Contracts.inventory;
using Common.Contracts.sales;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;

namespace Module.Auth.Application.UseCases.Branches.ToggleBranchStatus;

public class ToggleBranchStatus(
    IAuthDbContext context,
    ISalesIntegrationService salesService,
    IInventoryIntegrationService inventoryService)
{
    public async Task<Result<bool>> Execute(Guid id)
    {
        var branch = await context.Branches.FirstOrDefaultAsync(b => b.Id == id);
        if (branch == null) return ToggleBranchStatusErrors.BranchNotFound;

        if (branch.IsActive)
        {
            var hasOpenClosures = await salesService.BranchHasOpenClosures(id);
            if (hasOpenClosures)
                return ToggleBranchStatusErrors.BranchHasOpenClosures;

            var hasPendingTransfers = await inventoryService.BranchHasPendingTransfers(id);
            if (hasPendingTransfers)
                return ToggleBranchStatusErrors.BranchHasOpenClosures;
        }

        if (branch.IsActive)
            branch.Deactivate();
        else
            branch.Activate();

        await context.SaveChangesAsync();
        return true;
    }
}
