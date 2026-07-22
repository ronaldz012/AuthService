using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Products.Get;

public class GetProductsUc(IInvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<PagedResultDto<ListProductRequest>>> Execute(ProductQueryDto queryDto)
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
        
        var totalCount = await query.CountAsync();
        var items = await query
            //.OrderByDescending(p => p.CreatedAt)
            .ApplyPagination(queryDto)
            .Select(p => new ListProductRequest()
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
        return new PagedResultDto<ListProductRequest>()
        {
            TotalCount = totalCount,
            Items = items,
            Page = queryDto.Page,
            PageSize = queryDto.PageSize
        };
    }

}