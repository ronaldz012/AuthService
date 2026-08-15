using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Sales.Application.Abstraction;

namespace Module.Sales.Application.UseCases.Movements.List;

public class ListMovements(ISalesDbContext context)
{
    public async Task<Result<List<MovementListDto>>> Execute(ActorContext ctx)
    {
        var closure = await context.CashRegisterClosures
            .AsNoTracking()
            .Where(c => c.BranchId == ctx.BranchId && c.IsOpen)
            .Select(c => c.Id)
            .FirstOrDefaultAsync();

        if (closure == Guid.Empty)
            return ListMovementsErrors.NoOpenClosure;

        var movements = await context.CashRegisterMovements
            .AsNoTracking()
            .Where(m => m.CashRegisterClosureId == closure)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MovementListDto
            {
                Id = m.Id,
                CashRegisterClosureId = m.CashRegisterClosureId,
                Amount = m.Amount,
                Description = m.Description,
                Type = m.Type.ToString(),
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();

        return movements;
    }
}
