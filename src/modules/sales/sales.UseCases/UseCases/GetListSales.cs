using Auth.Contracts.Interfaces;
using Microsoft.EntityFrameworkCore;
using sales.Contracts.dtos;
using sales.Module.Data;

namespace sales.UseCases.UseCases;

public class GetListSales(SalesDbContext context,ICurrentUser currentUser )
{
    public async Task<List<SaleListDto>> Execute(SalesQueryDto queryDto)
    {
        var currentBranch = currentUser.BranchIds[0];
        
        var query = context.Sales
            .AsNoTracking() 
            .Where(s => s.BranchId == currentBranch);


        if (queryDto.DateFrom.HasValue)
            query = query.Where(s => s.CreatedAt >= queryDto.DateFrom.Value);
        
        if (queryDto.DateTo.HasValue)
            query = query.Where(s => s.CreatedAt <= queryDto.DateTo.Value);


        return await query
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SaleListDto
            {
                Id = s.Id,
                CreatedAt = s.CreatedAt,
                TotalAmount = s.TotalAmount,
                Status = s.Status,
                PaymentMethod = s.PaymentMethod,
                InvoiceNumber = s.InvoiceNumber,
                TransactionCode = s.TransactionCode,
            })
            .ToListAsync();
    }
    
}