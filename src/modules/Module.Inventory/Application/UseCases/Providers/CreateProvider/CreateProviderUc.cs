using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Organization;

namespace Module.Inventory.Application.UseCases.Providers.CreateProvider;

public class CreateProviderUc(IInvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<ProviderResponse>> Execute(CreateProviderRequest request)
    {
        var duplicate = await context.Providers
            .AnyAsync(p => p.Name == request.Name && p.IsActive);

        if (duplicate)
            return CreateProviderErrors.ProviderNameAlreadyExists;

        var provider = Provider.Create(
            request.Name,
            currentUser.TenantId,
            currentUser.UserId,
            currentUser.FullName,
            request.ContactName,
            request.Email,
            request.PhoneNumber,
            request.Address);

        context.Providers.Add(provider);
        await context.SaveChangesAsync();

        return ProviderResponse.FromEntity(provider);
    }
}
