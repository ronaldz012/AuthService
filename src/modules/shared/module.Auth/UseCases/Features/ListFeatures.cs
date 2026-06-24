using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using module.Auth.dtos.Features;

namespace module.Auth.Features.Features;

public class ListFeatures(AuthDbContext dbContext)
{
    public async Task<Result<PagedResultDto<FeatureDto>>> Execute(FeatureQueryDto queryDto)
    {
        var query = dbContext.Features.AsQueryable();
        if (!string.IsNullOrEmpty(queryDto.Filter))
            query = query.Where(m => m.Name.Contains(queryDto.Filter ?? string.Empty));

        (query, var totalCount) = query.ApplyFilters(queryDto);
        var result = await query.Select(x => new FeatureDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            Icon = x.Icon,
        }).ToListAsync();
        return new PagedResultDto<FeatureDto>
        {
            Items = result,
            Page = queryDto.GetPageValue(),
            PageSize = queryDto.GetPageSizeValue(),
            TotalCount = totalCount
        };
                    
    }
}
