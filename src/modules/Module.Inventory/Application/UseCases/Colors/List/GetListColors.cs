using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Colors.ListColors;

public class GetListColors(IInvDbContext context)
{
    public async Task<Result<PagedResultDto<ListColorResponse>>> Execute(ColoreQueryDto queryDto)
    {
        var query = context.Colors.AsQueryable();

        var (pagedQuery, totalCount)= query.ApplyFilters(queryDto);
        
        var colors = await  pagedQuery.Select(c => new ListColorResponse
        {
            Id = c.Id,
            Name = c.Name,
        }).ToListAsync();
        return new PagedResultDto<ListColorResponse>
        {
            Items = colors,
            TotalCount = totalCount,
            Page = queryDto.GetPageValue(),
            PageSize = queryDto.GetPageSizeValue()
        };
    }
}