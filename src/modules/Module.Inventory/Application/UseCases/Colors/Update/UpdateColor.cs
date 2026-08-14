using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Colors.Update;

public class UpdateColor(IInvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<bool>> Execute(Guid id, UpdateColorDto dto)
    {
        var color = await context.Colors.FirstOrDefaultAsync(c => c.Id == id);
        if (color is null)
            return UpdateColorErrors.ColorNotFound;

        var newName = dto.Name ?? color.Name;
        if (newName.ToLower() != color.Name.ToLower())
        {
            var duplicate = await context.Colors.AnyAsync(c =>
                c.Id != id && c.Name.ToLower() == newName.ToLower());

            if (duplicate)
                return UpdateColorErrors.ColorNameAlreadyExists;
        }

        color.Update(newName, currentUser.UserId, currentUser.FullName);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<Result<bool>> ChangeStatus(Guid id)
    {
        var color = await context.Colors.FirstOrDefaultAsync(c => c.Id == id);
        if (color is null)
            return UpdateColorErrors.ColorNotFound;

        color.ToggleActive(currentUser.UserId, currentUser.FullName);

        await context.SaveChangesAsync();
        return color.IsActive;
    }
}