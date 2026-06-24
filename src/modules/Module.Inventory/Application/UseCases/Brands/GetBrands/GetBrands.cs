using Common.Utilities;
using Inventory.Contracts.Dtos.Brands;
using Inventory.Data.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Inventory.Data;

namespace Inventory.UseCases.Brands;

public class GetBrands(InvDbContext context)
{
    public async Task<Result<PagedResultDto<BrandDto>>> Execute(QueryBrandDto query)
    {
        IQueryable<Brand> queryable =context.Brands;
        var (queryFiltered , totalCount) = queryable.ApplyFilters(query);
        var items = await queryFiltered.Select(x => new BrandDto()
        {
            Id = x.Id,
            Name = x.Name,
        }).ToListAsync();

        return new PagedResultDto<BrandDto>()
        {
            TotalCount = totalCount,
            Items = items,
            Page = query.GetPageValue(),
            PageSize = query.GetPageSizeValue()
        };
    }
}