using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Sales.Application.Abstraction;
using Module.Sales.Domain;

namespace Module.Sales.Application.UseCases.Registers.Open;

public static class OpenCashRegisterErrors
{
    public static readonly Error BranchMismatch = new(ErrorCode.Conflict, "Branch does not belong to the current user.");
    public static readonly Error AlreadyOpen = new(ErrorCode.Conflict, "There is already an open cash register for this branch.");
    public static readonly Error Failed = new(ErrorCode.InternalError, "Could not open cash register.");
}

public class OpenCashRegister(ISalesDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<Guid>> Execute(OpenCashRegisterDto dto)
    {
        if (dto.BranchId != currentUser.BranchId)
            return OpenCashRegisterErrors.BranchMismatch;

        var alreadyOpen = await context.CashRegisterClosures
            .AnyAsync(c => c.BranchId == dto.BranchId && c.Status == CashRegisterClosureStatus.Open);

        if (alreadyOpen)
            return OpenCashRegisterErrors.AlreadyOpen;

        var closure = new CashRegisterClosure
        {
            Id = Guid.NewGuid(),
            BranchId = dto.BranchId,
            OpenById = currentUser.UserId,
            OpenAt = DateTime.UtcNow,
            OpeningBalance = dto.OpeningBalance,
            Status = CashRegisterClosureStatus.Open
        };

        context.CashRegisterClosures.Add(closure);
        await context.SaveChangesAsync();

        return closure.Id;
    }
}
