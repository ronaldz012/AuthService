using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Organization;

namespace Module.Inventory.Application.UseCases.Providers.CreateProvider;

public class CreateProviderUc(IInvDbContext context)
{
    public async Task<Result<ProviderResponse>> Execute(ActorContext ctx, CreateProviderRequest request)
    {
        var duplicate = await context.Providers
            .AnyAsync(p => p.Name.ToLower() == request.Name.ToLower());

        if (duplicate)
            return CreateProviderErrors.ProviderNameAlreadyExists;

        var provider = Provider.Create(
            request.Name,
            ctx.TenantId,
            ctx.UserId,
            ctx.FullName,
            request.ContactName,
            request.Email,
            request.PhoneNumber,
            request.Address);

        context.Providers.Add(provider);
        await context.SaveChangesAsync();

        return ProviderResponse.FromEntity(provider);
    }
}
