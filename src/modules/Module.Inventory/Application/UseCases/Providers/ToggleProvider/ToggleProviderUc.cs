using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Providers.ToggleProvider;

public class ToggleProviderUc(IInvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<bool>> Execute(Guid id)
    {
        var provider = await context.Providers.FirstOrDefaultAsync(p => p.Id == id);
        if (provider is null)
            return ToggleProviderErrors.ProviderNotFound;

        provider.IsActive = !provider.IsActive;
        provider.UpdatedBy = currentUser.UserId;
        provider.UpdatedByName = currentUser.FullName;
        provider.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return provider.IsActive;
    }
}
