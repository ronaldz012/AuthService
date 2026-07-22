using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;

namespace Module.Auth.Application.UseCases.Features;

public class ListFeatures(IAuthDbContext dbContext)
{
    public async Task<Result<PagedResultDto<FeatureDto>>> Execute(FeatureQueryDto queryDto)
    {
        var query = dbContext.Features.AsQueryable();
        if (!string.IsNullOrEmpty(queryDto.Filter))
            query = query.Where(m => m.Key.Contains(queryDto.Filter ?? string.Empty));

        var totalCount = await query.CountAsync();
        var result = await query
            .OrderBy(f => f.Key)
            .ApplyPagination(queryDto)
            .Select(x => new FeatureDto
        {
    
            Name = x.Key,
            Description = x.Description,
            Icon = x.Icon,
        }).ToListAsync();
        return new PagedResultDto<FeatureDto>
        {
            Items = result,
            Page = queryDto.Page,
            PageSize = queryDto.PageSize,
            TotalCount = totalCount
        };
                    
    }
}
