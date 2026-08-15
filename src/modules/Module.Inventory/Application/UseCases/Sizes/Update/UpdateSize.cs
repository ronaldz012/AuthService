using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Sizes.Update;

public class UpdateSize(IInvDbContext context)
{
    public async Task<Result<bool>> Execute(ActorContext ctx, Guid id, UpdateSizeDto dto)
    {
        var size = await context.Sizes.FirstOrDefaultAsync(s => s.Id == id);
        if (size is null)
            return UpdateSizeErrors.SizeNotFound;

        var newName = dto.Name ?? size.Name;
        if (newName.ToLower() != size.Name.ToLower())
        {
            var duplicate = await context.Sizes.AnyAsync(s =>
                s.Id != id && s.Name.ToLower() == newName.ToLower());

            if (duplicate)
                return UpdateSizeErrors.SizeNameAlreadyExists;
        }

        size.Update(newName, dto.SortOrder ?? size.SortOrder, ctx.UserId, ctx.FullName);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<Result<bool>> ChangeStatus(ActorContext ctx, Guid id)
    {
        var size = await context.Sizes.FirstOrDefaultAsync(s => s.Id == id);
        if (size is null)
            return UpdateSizeErrors.SizeNotFound;

        size.ToggleActive(ctx.UserId, ctx.FullName);

        await context.SaveChangesAsync();
        return size.IsActive;
    }
}