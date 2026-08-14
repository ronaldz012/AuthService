using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Brands.GetBrands;

public class GetBrands(IInvDbContext context)
{
    public async Task<Result<List<ListBrandResponse>>> Execute(bool? includeInactive = null)
    {
        var query = context.Brands.AsNoTracking();

        if (includeInactive != true)
            query = query.Where(x => x.IsActive);

        var items = await query
            .OrderBy(x => x.Name)
            .Select(x => new ListBrandResponse
            {
                Id = x.Id,
                Name = x.Name,
                Prefix = x.Prefix,
                Description = x.Description,
                IsActive = x.IsActive,
            })
            .ToListAsync();

        return items;
    }
}