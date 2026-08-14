using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Categories.Get;

public class GetCategories(IInvDbContext context)
{
    public async Task<Result<List<GetCategoriesResponse>>> Execute(bool? includeInactive = null)
    {
        var query = context.Categories.AsNoTracking();

        if (includeInactive != true)
            query = query.Where(x => x.IsActive);

        var result = await query
            .OrderBy(x => x.Name)
            .Select(x => new GetCategoriesResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
            })
            .ToListAsync();

        return result;
    }
}