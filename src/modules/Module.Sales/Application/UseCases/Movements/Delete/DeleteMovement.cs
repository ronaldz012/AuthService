using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Sales.Application.Abstraction;

namespace Module.Sales.Application.UseCases.Movements.Delete;

public class DeleteMovement(ISalesDbContext context, ICurrentUser currentUser)
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

        movement.DeletedAt = DateTime.UtcNow;
        movement.DeletedBy = currentUser.UserId;
        movement.DeletedByName = currentUser.FullName;
        await context.SaveChangesAsync();

        return true;
    }
}
