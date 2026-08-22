using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Providers.Update;

public class UpdateProvider(IInvDbContext context)
{
    public async Task<Result<bool>> Execute(ActorContext ctx, Guid id, UpdateProviderRequest request)
    {
        var provider = await context.Providers.FirstOrDefaultAsync(p => p.Id == id);
        if (provider is null)
            return UpdateProviderErrors.ProviderNotFound;

        var newName = request.Name != null ? request.Name.Trim() : provider.Name;
        var normalizedName = newName.Trim().ToLowerInvariant();
        if (normalizedName != provider.Name.Trim().ToLowerInvariant())
        {
            var duplicate = await context.Providers
                .AnyAsync(p => p.Id != id && p.Name.ToLower() == normalizedName);

            if (duplicate)
                return UpdateProviderErrors.ProviderNameAlreadyExists;
        }

        provider.Update(
            newName,
            request.ContactName ?? provider.ContactName,
            request.Email ?? provider.Email,
            request.PhoneNumber ?? provider.PhoneNumber,
            request.Address ?? provider.Address,
            ctx.UserId,
            ctx.FullName);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<Result<bool>> ChangeStatus(ActorContext ctx, Guid id)
    {
        var provider = await context.Providers.FirstOrDefaultAsync(p => p.Id == id);
        if (provider is null)
            return UpdateProviderErrors.ProviderNotFound;

        provider.ToggleActive(ctx.UserId, ctx.FullName);

        await context.SaveChangesAsync();
        return provider.IsActive;
    }
}