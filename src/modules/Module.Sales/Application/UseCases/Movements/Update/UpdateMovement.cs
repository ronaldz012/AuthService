using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Sales.Application.Abstraction;
using Module.Sales.Domain;

namespace Module.Sales.Application.UseCases.Movements.Update;

public class UpdateMovement(ISalesDbContext context)
{
    public async Task<Result<bool>> Execute(Guid id, UpdateMovementDto dto)
    {
        var movement = await context.CashRegisterMovements
            .Include(m => m.CashRegisterClosure)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movement is null)
            return UpdateMovementErrors.NotFound;

        if (movement.CashRegisterClosure.Status != CashRegisterClosureStatus.Open)
            return UpdateMovementErrors.ClosureClosed;

        movement.Update(dto.Amount, dto.Description);

        await context.SaveChangesAsync();

        return true;
    }
}
