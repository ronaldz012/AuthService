using Auth.Contracts.Interfaces;
using Inventory.Contracts.Dtos.Receptions;
using Inventory.Data.Entities.Receptions;
using Inventory.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Extensions;
using Shared.Result;
using Shared.Services;

namespace Inventory.UseCases.Receptions;

public class ListReceptions(InvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<PagedResultDto<StockReceptionListDto>>> Execute(ReceptionQueryDto queryDto)
    {
        IQueryable<StockReception> query = context.StockReceptions;
        var branches = currentUser.BranchIds;

        if (queryDto.DateFrom.HasValue)
            query = query.Where(x => x.ReceivedAt >= queryDto.DateFrom.Value);

        if (queryDto.DateTo.HasValue)
            query = query.Where(x => x.ReceivedAt <= queryDto.DateTo.Value);


        if (queryDto.Status != null)
            query = query.Where(x => x.Status == queryDto.Status);

        if (queryDto.BrandId != null)
            query = query.Where(x =>
                x.Items.Any(i => i.ProductVariant.Product.BrandId == queryDto.BrandId));

        var (queryFiltered, totalCount) = query.ApplyFilters(queryDto);


        var receptions = await queryFiltered.Where(x => x.BranchId == currentUser.BranchIds[0])
            .Select(r => new StockReceptionListDto
            {
                Id = r.Id,
                BranchId = r.BranchId,
                ReceivedAt = r.ReceivedAt,
                Status = r.Status,
                TotalItems = r.Items.Sum(x => x.QuantityReceived),
                ProductVariantsCount = r.Items.Count,
                TotalCost = r.Items.Sum(i => i.UnitCost * i.QuantityReceived),
                BrandNames = r.Items.Select(x => x.ProductVariant.Product.Brand.Name).Distinct().ToList(),
                CategoryNames = r.Items.Select(x => x.ProductVariant.Product.Category.Name).Distinct().ToList()
            }).ToListAsync();
            

        return new PagedResultDto<StockReceptionListDto>
        {
            Items = receptions,
            TotalCount = totalCount,
            Page = queryDto.GetPageValue(),
            PageSize = queryDto.GetPageSizeValue()
        };

    }
}