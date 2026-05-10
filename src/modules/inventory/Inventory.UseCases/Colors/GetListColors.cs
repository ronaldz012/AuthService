using Common.Extensions;
using Common.Result;
using Inventory.Contracts.Dtos;
using Inventory.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.UseCases.Colors;

public class GetListColors(InvDbContext context)
{
    public async Task<Result<PagedResultDto<ColorDto>>> Execute(ColoreQueryDto queryDto)
    {
        var query = context.Colors.AsQueryable();

        var (pagedQuery, totalCount)= query.ApplyFilters(queryDto);
        
        var colors = await  pagedQuery.Select(c => new ColorDto
        {
            Id = c.Id,
            Name = c.Name,
            Code =  c.Code,
        }).ToListAsync();
        return new PagedResultDto<ColorDto>
        {
            Items = colors,
            TotalCount = totalCount,
            Page = queryDto.GetPageValue(),
            PageSize = queryDto.GetPageSizeValue()
        };
    }
}