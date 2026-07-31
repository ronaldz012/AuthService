using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Organization;

namespace Module.Inventory.Application.UseCases.Providers.GetProviders;

public class GetProviders(IInvDbContext context)
{
    public async Task<Result<List<ListProviderResponse>>> Execute()
    {
        var items = await context.Providers
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new ListProviderResponse
            {
                Id = p.Id,
                Name = p.Name,
                ContactName = p.ContactName,
                Email = p.Email,
                PhoneNumber = p.PhoneNumber,
                Address = p.Address
            })
            .ToListAsync();

        return items;
    }
}
