using Inventory.Contracts.Dtos.Products;
using Inventory.Contracts.Dtos.ProductVariants;
using Inventory.Data.Persistence;
using Shared.Result;

namespace Inventory.UseCases.ProductVariants;

public class UpdateProductVariant(InvDbContext context)
{
    public async Task<Result<bool>> Execute(UpdateProductVariantDto dto, int id)
    {
        var productVariant = await context.ProductVariants.FindAsync(id);

        if (productVariant is null)
            return new Error("NOT_FOUND", "product variant not found");
        
        productVariant.MapTo(dto);
        await context.SaveChangesAsync();
        return true;
    }
}