using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Products.UpdateStatus;

public class UpdateProductStatus(IInvDbContext context)
{
    public async Task<Result<bool>> Execute(ActorContext ctx, Guid id, UpdateProductStatusDto dto)
    {
        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
            return UpdateProductStatusErrors.ProductNotFound;

        product.IsActive = dto.IsActive;
        product.UpdatedBy = ctx.UserId;
        product.UpdatedByName = ctx.FullName;
        product.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return product.IsActive;
    }
}