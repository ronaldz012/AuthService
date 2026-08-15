using Common.Contracts.authentication;
using Common.Utilities;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.ProductVariants.Update;

public class UpdateProductVariant(IInvDbContext context)
{
    public async Task<Result<bool>> Execute(ActorContext ctx, UpdateProductVariantDto dto, Guid id)
    {
        var productVariant = await context.ProductVariants.FindAsync(id);

        if (productVariant is null)
            return UpdateProductVariantErrors.VariantNotFound;
        
        productVariant.Description = dto.Description ?? productVariant.Description;
        productVariant.Price = dto.Price ?? productVariant.Price;
        productVariant.UpdatedBy = ctx.UserId;
        productVariant.UpdatedByName = ctx.FullName;
        productVariant.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }
}