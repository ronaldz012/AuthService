using Common.Contracts.authentication;
using Common.Contracts.inventory;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Sales.Application.Abstraction;

namespace Module.Sales.Application.UseCases.Registers.LastClosure;

public class GetLastClosureSummary(
    ISalesDbContext context,
    IInventoryIntegrationService inventoryService,
    ICurrentUser currentUser)
{
    public async Task<Result<LastClosureSummaryDto>> Execute()
    {
        var closure = await context.CashRegisterClosures
            .AsNoTracking()
            .Where(c => c.BranchId == currentUser.BranchId && !c.IsOpen)
            .OrderByDescending(c => c.OpenAt)
            .Select(c => new
            {
                c.Id,
                c.OpenAt,
                c.ClosedAt,
                TotalSales = c.Sales.Sum(s => s.TotalAmount),
                SalesCount = c.Sales.Count(),
                ItemsSold = c.Sales.Sum(s => s.SaleItems.Sum(si => si.Quantity))
            })
            .FirstOrDefaultAsync();

        if (closure is null)
            return new LastClosureSummaryDto { HasData = false };

        var variantIds = await context.Sales
            .AsNoTracking()
            .Where(s => s.CashRegisterClosureId == closure.Id && s.BranchId == currentUser.BranchId)
            .SelectMany(s => s.SaleItems)
            .Select(si => si.ProductVariantId)
            .Distinct()
            .ToListAsync();

        var stockResult = await inventoryService.GetVariantsWithStock(variantIds, currentUser.BranchId);
        if (!stockResult.IsSuccess)
            return stockResult.Error;

        var restockCount = stockResult.Value.Count(v => v.Stock <= 0);

        return new LastClosureSummaryDto
        {
            HasData = true,
            ClosureId = closure.Id,
            OpenedAt = closure.OpenAt,
            ClosedAt = closure.ClosedAt,
            TotalSales = closure.TotalSales,
            SalesCount = closure.SalesCount,
            ItemsSold = closure.ItemsSold,
            RestockCount = restockCount
        };
    }
}
