using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Providers.Update;

public class UpdateProvider(IInvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<bool>> Execute(Guid id, UpdateProviderRequest request)
    {
        var provider = await context.Providers.FirstOrDefaultAsync(p => p.Id == id);
        if (provider is null)
            return UpdateProviderErrors.ProviderNotFound;

        var newName = request.Name ?? provider.Name;
        if (newName.ToLower() != provider.Name.ToLower())
        {
            var duplicate = await context.Providers
                .AnyAsync(p => p.Id != id && p.Name.ToLower() == newName.ToLower());

            if (duplicate)
                return UpdateProviderErrors.ProviderNameAlreadyExists;
        }

        provider.Update(
            newName,
            request.ContactName ?? provider.ContactName,
            request.Email ?? provider.Email,
            request.PhoneNumber ?? provider.PhoneNumber,
            request.Address ?? provider.Address,
            currentUser.UserId,
            currentUser.FullName);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<Result<bool>> ChangeStatus(Guid id)
    {
        var provider = await context.Providers.FirstOrDefaultAsync(p => p.Id == id);
        if (provider is null)
            return UpdateProviderErrors.ProviderNotFound;

        provider.ToggleActive(currentUser.UserId, currentUser.FullName);

        await context.SaveChangesAsync();
        return provider.IsActive;
    }
}