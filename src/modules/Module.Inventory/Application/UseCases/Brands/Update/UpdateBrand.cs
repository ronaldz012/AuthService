using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Brands.Update;

public class UpdateBrand(IInvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<bool>> Execute(Guid id, UpdateBrandDto dto)
    {
        var brand = await context.Brands.FirstOrDefaultAsync(b => b.Id == id);
        if (brand is null)
            return UpdateBrandErrors.BrandNotFound;

        var newName = dto.Name ?? brand.Name;
        if (newName.ToLower() != brand.Name.ToLower())
        {
            var duplicate = await context.Brands.AnyAsync(b =>
                b.Id != id && b.Name.ToLower() == newName.ToLower());

            if (duplicate)
                return UpdateBrandErrors.BrandNameAlreadyExists;
        }

        brand.Update(newName, dto.Description ?? brand.Description, currentUser.UserId, currentUser.FullName);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<Result<bool>> ChangeStatus(Guid id)
    {
        var brand = await context.Brands.FirstOrDefaultAsync(b => b.Id == id);
        if (brand is null)
            return UpdateBrandErrors.BrandNotFound;

        brand.ToggleActive(currentUser.UserId, currentUser.FullName);

        await context.SaveChangesAsync();
        return brand.IsActive;
    }
}