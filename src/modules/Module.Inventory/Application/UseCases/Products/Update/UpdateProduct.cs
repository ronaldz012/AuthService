using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Products.Update;

public class UpdateProduct(IInvDbContext context)
{
    public async Task<Result<bool>> Execute(ActorContext ctx, UpdateProductDto dto, Guid id)
    {
        var product = await context.Products.FindAsync(id);
        if(product == null) return 
            UpdateProductErrors.ProductNotFound;

        if (dto.Name is not null)
        {
            var effectiveCategoryId = dto.CategoryId ?? product.CategoryId;
            var normalizedName = dto.Name.Trim().ToLowerInvariant();
            var duplicateName = await context.Products.AnyAsync(p =>
                p.Id != id &&
                p.CategoryId == effectiveCategoryId &&
                p.BrandId == product.BrandId &&
                p.Name.ToLower() == normalizedName);

            if (duplicateName)
                return UpdateProductErrors.ProductNameAlreadyExists;
        }

        product.Name = dto.Name != null ? dto.Name.Trim() : product.Name;
        product.Description = dto.Description != null ? dto.Description.Trim() : product.Description;
        product.Gender = dto.Gender ?? product.Gender;
        product.CategoryId = dto.CategoryId ?? product.CategoryId;
        product.UpdatedBy = ctx.UserId;
        product.UpdatedByName = ctx.FullName;
        product.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;

    }
}