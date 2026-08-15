using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Users.UpdateUserStatus;

public class UpdateUserStatus(IAuthDbContext context)
{
    public async Task<Result<bool>> Execute(ActorContext ctx, Guid id)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return UpdateUserStatusErrors.UserNotFound;

        if (user.IsActive)
            user.Deactivate(ctx.UserId, ctx.FullName);
        else
            user.Activate(ctx.UserId, ctx.FullName);

        await context.SaveChangesAsync();
        return true;
    }
}
