using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Colors.List;

public class GetListColors(IInvDbContext context)
{
    public async Task<Result<List<ListColorResponse>>> Execute()
    {
        var colors = await context.Colors
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new ListColorResponse
            {
                Id = c.Id,
                Name = c.Name,
            })
            .ToListAsync();

        return colors;
    }
}