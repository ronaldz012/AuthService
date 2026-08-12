using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Sizes.List;

public class GetListSizes(IInvDbContext context)
{
    public async Task<Result<List<ListSizeResponse>>> Execute()
    {
        var sizes = await context.Sizes
            .AsNoTracking()
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .Select(s => new ListSizeResponse
            {
                Id = s.Id,
                Name = s.Name,
                SortOrder = s.SortOrder
            })
            .ToListAsync();

        return sizes;
    }
}