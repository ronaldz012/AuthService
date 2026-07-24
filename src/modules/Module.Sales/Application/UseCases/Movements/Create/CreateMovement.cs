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
            .FirstOrDefaultAsync(c => c.BranchId == currentUser.BranchId && c.IsOpen);

        if (closure is null)
            return CreateMovementErrors.NoOpenClosure;

        var movement = CashRegisterMovement.Create(
            closure.Id, dto.Amount, dto.Description, CashRegisterMovementType.Outflow, currentUser.UserId, currentUser.FullName);

        context.CashRegisterMovements.Add(movement);
        await context.SaveChangesAsync();

        return movement.Id;
    }
}
