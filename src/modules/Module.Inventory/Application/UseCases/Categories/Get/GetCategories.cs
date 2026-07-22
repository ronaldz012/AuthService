using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Categories.Get;

public class GetCategories(IInvDbContext context)
{
    public async Task<Result<List<GetCategoriesResponse>>> Execute()
    {
        var result = await context.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new GetCategoriesResponse
            {
                Id = x.Id,
                Name = x.Name,
            })
            .ToListAsync();

        return result;
    }
}