using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Sales.Application.Abstraction;
using Module.Sales.Domain;

namespace Module.Sales.Application.UseCases.Registers.Close;

public class CloseCashRegister(ISalesDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<CloseCashRegisterResponseDto>> Execute(CloseCashRegisterDto dto)
    {
        var closure = await context.CashRegisterClosures
            .Include(c => c.Sales)
            .Include(c => c.Movements)
            .FirstOrDefaultAsync(c => c.BranchId == currentUser.BranchId && c.Status == CashRegisterClosureStatus.Open);

        if (closure is null)
            return CloseCashRegisterErrors.NotFound;

        closure.Close(dto.RealCountedAmount, currentUser.UserId);

        await context.SaveChangesAsync();

        var cashSalesTotal = closure.Sales
            .Where(s => s.PaymentMethod == PaymentMethod.Cash)
            .Sum(s => s.TotalAmount);

        var outflowsTotal = closure.Movements
            .Where(m => m.Type == CashRegisterMovementType.Outflow)
            .Sum(m => m.Amount);

        return new CloseCashRegisterResponseDto
        {
            Id = closure.Id,
            OpeningBalance = closure.OpeningBalance,
            CashSalesTotal = cashSalesTotal,
            OutflowsTotal = outflowsTotal,
            ExpectedCash = closure.SystemSalesAmount,
            RealCountedAmount = closure.RealCountedAmount,
            Difference = closure.RealCountedAmount - closure.SystemSalesAmount,
            ClosedAt = closure.ClosedAt!.Value
        };
    }
}
