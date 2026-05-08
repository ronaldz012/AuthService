using Inventory.Contracts.Dtos.Categories;
using Inventory.Data.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Common.Extensions;
using Common.Result;
using Inventory.Data;

namespace Inventory.UseCases.Categories;


public class GetCategories(InvDbContext context)
{
    public async Task<Result<PagedResultDto<CategoryDto>>> Execute(CategoryQueryDto queryDto)
    {
        IQueryable<Category> query = context.Categories;

        var (filteredQuery, totalCount) = query.ApplyFilters(queryDto);

        var result = await filteredQuery.Select(x => new CategoryDto()
        {
            Id = x.Id,
            Name = x.Name,
        }).ToListAsync();

        return new PagedResultDto<CategoryDto>()
        {
            TotalCount = totalCount,
            Items = result,
            Page = queryDto.GetPageSizeValue(),
            PageSize = queryDto.GetPageSizeValue()
        };

    }
}