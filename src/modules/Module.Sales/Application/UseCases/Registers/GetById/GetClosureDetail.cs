using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Sales.Application.Abstraction;
using Module.Sales.Domain;

namespace Module.Sales.Application.UseCases.Registers.GetById;

public class GetClosureDetail(ISalesDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<ClosureDetailDto>> Execute(Guid id)
    {
        var closure = await context.CashRegisterClosures
            .AsNoTracking()
            .Where(c => c.Id == id && c.BranchId == currentUser.BranchId)
            .Select(c => new ClosureDetailDto
            {
                Id = c.Id,
                BranchId = c.BranchId,
                OpenedAt = c.OpenAt,
                ClosedAt = c.ClosedAt,
                OpenedByName = c.OpenByName,
                ClosedByName = c.CloseByName,
                OpeningBalance = c.OpeningBalance,
                SystemSalesAmount = c.SystemSalesAmount,
                RealCountedAmount = c.RealCountedAmount,
                Difference = c.RealCountedAmount - c.SystemSalesAmount,
                TotalSales = c.Sales.Sum(s => s.TotalAmount),
                CashSales = c.Sales.Where(s => s.PaymentMethod == PaymentMethod.Cash).Sum(s => s.TotalAmount),
                TotalExpenses = c.Movements.Where(m => m.Type == CashRegisterMovementType.Outflow).Sum(m => m.Amount),
                Sales = c.Sales
                    .OrderByDescending(s => s.CreatedAt)
                    .Select(s => new ClosureSaleItemDto
                    {
                        Id = s.Id,
                        CreatedAt = s.CreatedAt,
                        SoldByName = s.SoldByName,
                        TotalAmount = s.TotalAmount,
                        PaymentMethod = s.PaymentMethod.ToString(),
                        DocumentType = s.DocumentType.ToString(),
                        InvoiceNumber = s.InvoiceNumber,
                        TransactionCode = s.TransactionCode,
                        ItemsCount = s.SaleItems.Count
                    }).ToList(),
                Movements = c.Movements
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new ClosureMovementDto
                    {
                        Id = m.Id,
                        CreatedAt = m.CreatedAt,
                        Amount = m.Amount,
                        Description = m.Description,
                        Type = m.Type.ToString()
                    }).ToList()
            })
            .FirstOrDefaultAsync();

        if (closure is null)
            return GetClosureErrors.NotFound;

        return closure;
    }
}
