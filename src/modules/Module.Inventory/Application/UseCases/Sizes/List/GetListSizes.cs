using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Sizes.List;

public class GetListSizes(IInvDbContext context)
{
    public async Task<Result<List<ListSizeResponse>>> Execute(bool? includeInactive = null)
    {
        var query = context.Sizes.AsNoTracking();

        if (includeInactive != true)
            query = query.Where(x => x.IsActive);

        var sizes = await query
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .Select(s => new ListSizeResponse
            {
                Id = s.Id,
                Name = s.Name,
                SortOrder = s.SortOrder,
                IsActive = s.IsActive,
            })
            .ToListAsync();

        return sizes;
    }
}