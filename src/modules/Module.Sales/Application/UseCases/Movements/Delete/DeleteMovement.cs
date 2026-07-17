using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Sales.Application.Abstraction;

namespace Module.Sales.Application.UseCases.Movements.Delete;

public class DeleteMovement(ISalesDbContext context)
{
    public async Task<Result<bool>> Execute(Guid id)
    {
        var movement = await context.CashRegisterMovements
            .Include(m => m.CashRegisterClosure)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movement is null)
            return DeleteMovementErrors.NotFound;

        if (!movement.CashRegisterClosure.IsOpen)
            return DeleteMovementErrors.ClosureClosed;

        context.CashRegisterMovements.Remove(movement);
        await context.SaveChangesAsync();

        return true;
    }
}
