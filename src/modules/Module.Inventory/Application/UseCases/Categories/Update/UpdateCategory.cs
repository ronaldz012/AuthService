using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Categories.Update;

public class UpdateCategory(IInvDbContext context)
{
    public async Task<Result<bool>> Execute(ActorContext ctx, Guid id, UpdateCategoryDto dto)
    {
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
            return UpdateCategoryErrors.CategoryNotFound;

        var newName = dto.Name ?? category.Name;
        if (newName.ToLower() != category.Name.ToLower())
        {
            var duplicate = await context.Categories.AnyAsync(c =>
                c.Id != id && c.Name.ToLower() == newName.ToLower());

            if (duplicate)
                return UpdateCategoryErrors.CategoryNameAlreadyExists;
        }

        category.Update(newName, dto.Description ?? category.Description, ctx.UserId, ctx.FullName);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<Result<bool>> ChangeStatus(ActorContext ctx, Guid id)
    {
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
            return UpdateCategoryErrors.CategoryNotFound;

        category.ToggleActive(ctx.UserId, ctx.FullName);

        await context.SaveChangesAsync();
        return category.IsActive;
    }
}