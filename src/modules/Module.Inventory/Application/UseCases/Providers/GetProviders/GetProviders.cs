using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Organization;

namespace Module.Inventory.Application.UseCases.Providers.GetProviders;

public class GetProviders(IInvDbContext context)
{
    public async Task<Result<List<ListProviderResponse>>> Execute(bool? includeInactive = null)
    {
        var query = context.Providers.AsNoTracking();

        if (includeInactive != true)
            query = query.Where(p => p.IsActive);

        var items = await query
            .OrderBy(p => p.Name)
            .Select(p => new ListProviderResponse
            {
                Id = p.Id,
                Name = p.Name,
                ContactName = p.ContactName,
                Email = p.Email,
                PhoneNumber = p.PhoneNumber,
                Address = p.Address,
                IsActive = p.IsActive
            })
            .ToListAsync();

        return items;
    }
}
