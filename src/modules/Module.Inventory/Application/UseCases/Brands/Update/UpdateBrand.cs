using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Brands.Update;

public class UpdateBrand(IInvDbContext context)
{
    public async Task<Result<bool>> Execute(ActorContext ctx, Guid id, UpdateBrandDto dto)
    {
        var brand = await context.Brands.FirstOrDefaultAsync(b => b.Id == id);
        if (brand is null)
            return UpdateBrandErrors.BrandNotFound;

        var newName = dto.Name != null ? dto.Name.Trim() : brand.Name;
        var normalizedName = newName.Trim().ToLowerInvariant();
        if (normalizedName != brand.Name.Trim().ToLowerInvariant())
        {
            var duplicate = await context.Brands.AnyAsync(b =>
                b.Id != id && b.Name.ToLower() == normalizedName);

            if (duplicate)
                return UpdateBrandErrors.BrandNameAlreadyExists;
        }

        brand.Update(newName, dto.Description ?? brand.Description, ctx.UserId, ctx.FullName);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<Result<bool>> ChangeStatus(ActorContext ctx, Guid id)
    {
        var brand = await context.Brands.FirstOrDefaultAsync(b => b.Id == id);
        if (brand is null)
            return UpdateBrandErrors.BrandNotFound;

        brand.ToggleActive(ctx.UserId, ctx.FullName);

        await context.SaveChangesAsync();
        return brand.IsActive;
    }
}