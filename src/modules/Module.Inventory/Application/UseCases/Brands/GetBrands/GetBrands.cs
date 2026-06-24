using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Brands.GetBrands;

public class GetBrands(IInvDbContext context)
{
    public async Task<Result<PagedResultDto<ListBrandResponse>>> Execute(QueryBrandDto query)
    {
        IQueryable<Brand> queryable =context.Brands;
        var (queryFiltered , totalCount) = queryable.ApplyFilters(query);
        var items = await queryFiltered.Select(x => new ListBrandResponse()
        {
            Id = x.Id,
            Name = x.Name,
        }).ToListAsync();

        return new PagedResultDto<ListBrandResponse>()
        {
            TotalCount = totalCount,
            Items = items,
            Page = query.GetPageValue(),
            PageSize = query.GetPageSizeValue()
        };
    }
}