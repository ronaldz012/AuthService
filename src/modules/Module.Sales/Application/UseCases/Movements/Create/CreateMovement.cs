using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Sales.Application.Abstraction;
using Module.Sales.Domain;

namespace Module.Sales.Application.UseCases.Movements.Create;

public class CreateMovement(ISalesDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<Guid>> Execute(CreateMovementDto dto)
    {
        var closure = await context.CashRegisterClosures
            .FirstOrDefaultAsync(c => c.Id == dto.CashRegisterClosureId);

        if (closure is null || closure.BranchId != currentUser.BranchId)
            return CreateMovementErrors.ClosureNotFound;

        if (closure.Status != CashRegisterClosureStatus.Open)
            return CreateMovementErrors.ClosureNotOpen;

        var movement = CashRegisterMovement.Create(
            dto.CashRegisterClosureId, dto.Amount, dto.Description, dto.Type);

        context.CashRegisterMovements.Add(movement);
        await context.SaveChangesAsync();

        return movement.Id;
    }
}
