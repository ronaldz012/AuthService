using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Sales.Application.Abstraction;

namespace Module.Sales.Application.UseCases.Movements.Delete;

public class DeleteMovement(ISalesDbContext context)
{
    public async Task<Result<bool>> Execute(ActorContext ctx, Guid id)
    {
        var movement = await context.CashRegisterMovements
            .Include(m => m.CashRegisterClosure)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movement is null)
            return DeleteMovementErrors.NotFound;

        if (!movement.CashRegisterClosure.IsOpen)
            return DeleteMovementErrors.ClosureClosed;

        movement.DeletedAt = DateTime.UtcNow;
        movement.DeletedBy = ctx.UserId;
        movement.DeletedByName = ctx.FullName;
        await context.SaveChangesAsync();

        return true;
    }
}
