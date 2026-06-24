using Common.Contracts.authentication;
using Inventory.Contracts.Dtos.Products;
using Inventory.Data.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Common.Services;
using Common.Utilities;
using Inventory.Data;

namespace Inventory.UseCases.Products;

public class ListProducts(InvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<PagedResultDto<ListProductDto>>> Execute(ProductQueryDto queryDto)
    {
        IQueryable<Product> query = context.Products;
        if (!string.IsNullOrEmpty(queryDto.Filter))
        {
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{queryDto.Filter}%"));
        }

        if (queryDto.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == queryDto.CategoryId);
        }
        if(queryDto.BrandId.HasValue)
            query = query.Where(x => x.BrandId == queryDto.BrandId);

        if (queryDto.Gender.HasValue)
        {
            query = query.Where(x => x.Gender == queryDto.Gender);
        }
        //if(queryDto.LowStock.HasValue) PARA IMPLEMENTAR::::
        
        var (filteredQuery, totalCount) = query.ApplyFilters(queryDto);
        var items = await filteredQuery.Select(p => new ListProductDto()
        {
            Id = p.Id,
            Name = p.Name,
            BrandName = p.Brand.Name,
            CategoryName = p.Category.Name,
            BasePrice = p.BasePrice,
            InternalCode = p.InternalCode,

            VariantsCount = p.ProductVariants.Count,
            TotalStock = p.ProductVariants
                .SelectMany(pv => pv.BranchInventories)
                .Where(bi => currentUser.BranchIds.Contains(bi.BranchId))
                .Sum(bi => bi.Stock),
        }).ToListAsync();
        return new PagedResultDto<ListProductDto>()
        {
            TotalCount = totalCount,
            Items = items,
            Page = queryDto.GetPageValue(),
            PageSize = queryDto.GetPageSizeValue()
        };
    }

}