using Common.Contracts.sales;
using Microsoft.EntityFrameworkCore;
using Module.Sales.Application.Abstraction;
using Module.Sales.Domain;

namespace Module.Sales.Infrastructure;

public class SalesIntegrationService(ISalesDbContext context) : ISalesIntegrationService
{
    public async Task<bool> BranchHasOpenClosures(Guid branchId)
    {
        return await context.CashRegisterClosures
            .AnyAsync(c => c.BranchId == branchId && c.Status == CashRegisterClosureStatus.Open);
    }
}
