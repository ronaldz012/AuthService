using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Categories.Update;

public class UpdateCategory(IInvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<bool>> Execute(Guid id, UpdateCategoryDto dto)
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

        category.Update(newName, dto.Description ?? category.Description, currentUser.UserId, currentUser.FullName);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<Result<bool>> ChangeStatus(Guid id)
    {
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
            return UpdateCategoryErrors.CategoryNotFound;

        category.ToggleActive(currentUser.UserId, currentUser.FullName);

        await context.SaveChangesAsync();
        return category.IsActive;
    }
}