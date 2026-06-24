using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Categories.GetCategories;


public class GetCategories(IInvDbContext context)
{
    public async Task<Result<PagedResultDto<GetCategoriesResponse>>> Execute(CategoryQueryDto queryDto)
    {
        IQueryable<Category> query = context.Categories;

        var (filteredQuery, totalCount) = query.ApplyFilters(queryDto);

        var result = await filteredQuery.Select(x => new GetCategoriesResponse()
        {
            Id = x.Id,
            Name = x.Name,
        }).ToListAsync();

        return new PagedResultDto<GetCategoriesResponse>()
        {
            TotalCount = totalCount,
            Items = result,
            Page = queryDto.GetPageSizeValue(),
            PageSize = queryDto.GetPageSizeValue()
        };

    }
}