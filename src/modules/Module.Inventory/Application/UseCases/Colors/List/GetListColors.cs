using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Colors.List;

public class GetListColors(IInvDbContext context)
{
    public async Task<Result<List<ListColorResponse>>> Execute(bool? includeInactive = null)
    {
        var query = context.Colors.AsNoTracking();

        if (includeInactive != true)
            query = query.Where(x => x.IsActive);

        var colors = await query
            .OrderBy(c => c.Name)
            .Select(c => new ListColorResponse
            {
                Id = c.Id,
                Name = c.Name,
                IsActive = c.IsActive,
            })
            .ToListAsync();

        return colors;
    }
}