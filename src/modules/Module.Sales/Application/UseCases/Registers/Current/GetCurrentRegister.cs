using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Sales.Application.Abstraction;

namespace Module.Sales.Application.UseCases.Registers.Current;

public class GetCurrentRegister(ISalesDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<CurrentRegisterDto>> Execute()
    {
        var closure = await context.CashRegisterClosures
            .AsNoTracking()
            .Where(c => c.BranchId == currentUser.BranchId && c.IsOpen)
            .Select(c => new CurrentRegisterDto
            {
                IsOpen = true,
                ClosureId = c.Id,
                OpeningBalance = c.OpeningBalance,
                OpenedAt = c.OpenAt,
                OpenByName = c.OpenByName
            })
            .FirstOrDefaultAsync();

        return closure ?? new CurrentRegisterDto { IsOpen = false };
    }
}
