using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Sales.Application.Abstraction;

namespace Module.Sales.Application.UseCases.Sales.Get;

public class GetListSales(ISalesDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<PagedResultDto<SaleListDto>>> Execute(SalesQueryDto queryDto)
    {
        var currentBranch = currentUser.BranchIds[0];
        
        var query = context.Sales
            .AsNoTracking() 
            .Where(s => s.BranchId == currentBranch);


        if (queryDto.DateFrom.HasValue)
            query = query.Where(s => s.CreatedAt >= queryDto.DateFrom.Value);
        
        if (queryDto.DateTo.HasValue)
            query = query.Where(s => s.CreatedAt <= queryDto.DateTo.Value);


        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SaleListDto
            {
                Id = s.Id,
                CreatedAt = s.CreatedAt,
                TotalAmount = s.TotalAmount,
                DocumentType = s.DocumentType,
                PaymentMethod = s.PaymentMethod,
                InvoiceNumber = s.InvoiceNumber,
                TransactionCode = s.TransactionCode,
                ItemCount = s.SaleItems.Count
            })
            .ToListAsync();

        return new PagedResultDto<SaleListDto>
        {
            Items = items,
            TotalCount = items.Count,
            PageSize = queryDto.GetPageSizeValue(),
            Page = queryDto.GetPageValue()
        };
    }
    
}