using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Sales.Application.Abstraction;

namespace Module.Sales.Application.UseCases.Movements.Update;

public class UpdateMovement(ISalesDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<bool>> Execute(Guid id, UpdateMovementDto dto)
    {
        var movement = await context.CashRegisterMovements
            .Include(m => m.CashRegisterClosure)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movement is null)
            return UpdateMovementErrors.NotFound;

        if (!movement.CashRegisterClosure.IsOpen)
            return UpdateMovementErrors.ClosureClosed;

        movement.Update(dto.Amount, dto.Description, currentUser.UserId, currentUser.FullName);

        await context.SaveChangesAsync();

        return true;
    }
}
