using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Brands.GetBrands;

public class GetBrands(IInvDbContext context)
{
    public async Task<Result<List<ListBrandResponse>>> Execute()
    {
        var items = await context.Brands
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ListBrandResponse
            {
                Id = x.Id,
                Name = x.Name,
            })
            .ToListAsync();

        return items;
    }
}