using Common.Utilities;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.ProductVariants.Update;

public class UpdateProductVariant(IInvDbContext context)
{
    public async Task<Result<bool>> Execute(UpdateProductVariantDto dto, Guid id)
    {
        var productVariant = await context.ProductVariants.FindAsync(id);

        if (productVariant is null)
            return UpdateProductVariantErrors.VariantNotFound;
        
        productVariant.Description = dto.Description ?? productVariant.Description;
        productVariant.Price = dto.Price ?? productVariant.Price;
        await context.SaveChangesAsync();
        return true;
    }
}