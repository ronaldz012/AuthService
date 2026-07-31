using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Providers.UpdateProvider;

public class UpdateProviderUc(IInvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<bool>> Execute(Guid id, UpdateProviderRequest request)
    {
        var provider = await context.Providers.FirstOrDefaultAsync(p => p.Id == id);
        if (provider is null)
            return UpdateProviderErrors.ProviderNotFound;

        var duplicate = await context.Providers
            .AnyAsync(p => p.Id != id && p.Name.ToLower() == (request.Name ?? string.Empty).ToLower());

        if (duplicate)
            return UpdateProviderErrors.ProviderNameAlreadyExists;

        provider.Name = request.Name ?? provider.Name;
        provider.ContactName = request.ContactName ?? provider.ContactName;
        provider.Email = request.Email ?? provider.Email;
        provider.PhoneNumber = request.PhoneNumber ?? provider.PhoneNumber;
        provider.Address = request.Address ?? provider.Address;
        provider.UpdatedBy = currentUser.UserId;
        provider.UpdatedByName = currentUser.FullName;
        provider.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return true;
    }
}
