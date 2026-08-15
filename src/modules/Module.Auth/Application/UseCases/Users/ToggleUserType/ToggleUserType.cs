using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Users.ToggleUserType;

public class ToggleUserType(IAuthDbContext context)
{
    public async Task<Result<bool>> Execute(ActorContext ctx, Guid id)
    {
        var user = await context.Users
            .Include(u => u.UserBranchRoles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return ToggleUserTypeErrors.UserNotFound;

        if (user.Type == UserType.Owner)
            return ToggleUserTypeErrors.CannotToggleOwner;

        if (user.Type == UserType.TenantAdmin)
        {
            if (!user.CanDemoteToStandard())
                return ToggleUserTypeErrors.NoBranchRolesAssigned;
            user.DemoteToStandard(ctx.UserId, ctx.FullName);
        }
        else
        {
            user.PromoteToAdmin(ctx.UserId, ctx.FullName);
        }

        await context.SaveChangesAsync();
        return true;
    }
}
