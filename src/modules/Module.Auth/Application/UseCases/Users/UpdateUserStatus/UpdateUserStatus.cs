using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Users.UpdateUserStatus;

public class UpdateUserStatus(IAuthDbContext context)
{
    public async Task<Result<bool>> Execute(Guid id)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return UpdateUserStatusErrors.UserNotFound;

        user.Status = user.Status == UserStatus.Active ? UserStatus.InActive : UserStatus.Active;
        user.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }
}
