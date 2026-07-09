using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Users.ToggleUserType;

public class ToggleUserType(IAuthDbContext context)
{
    public async Task<Result<bool>> Execute(Guid id)
    {
        var user = await context.Users
            .Include(u => u.UserBranchRoles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return ToggleUserTypeErrors.UserNotFound;

        if (user.Type == UserType.Owner)
            return ToggleUserTypeErrors.CannotToggleOwner;

        if (user.Type == UserType.TenantAdmin && user.UserBranchRoles.Count == 0)
            return ToggleUserTypeErrors.NoBranchRolesAssigned;

        user.Type = user.Type == UserType.TenantAdmin
            ? UserType.Standard
            : UserType.TenantAdmin;

        user.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return true;
    }
}
