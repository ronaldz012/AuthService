using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Colors.Update;

public class UpdateColor(IInvDbContext context)
{
    public async Task<Result<bool>> Execute(ActorContext ctx, Guid id, UpdateColorDto dto)
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

        color.Update(newName, ctx.UserId, ctx.FullName);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<Result<bool>> ChangeStatus(ActorContext ctx, Guid id)
    {
        var color = await context.Colors.FirstOrDefaultAsync(c => c.Id == id);
        if (color is null)
            return UpdateColorErrors.ColorNotFound;

        color.ToggleActive(ctx.UserId, ctx.FullName);

        await context.SaveChangesAsync();
        return color.IsActive;
    }
}