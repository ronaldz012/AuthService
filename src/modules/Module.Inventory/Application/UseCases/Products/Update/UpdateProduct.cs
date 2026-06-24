using Common.Utilities;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Products.Update;

public class UpdateProduct(IInvDbContext context)
{
    public async Task<Result<bool>> Execute(UpdateProductDto dto, Guid id)
    {
        var product = await context.Products.FindAsync(id);
        if(product == null) return 
            new Error("NOT_FOUND", "Product not found");
        
        product.Name = dto.Name ?? product.Name;
        product.Description = dto.Description ?? product.Description;
        product.Gender = dto.Gender ?? product.Gender;
        product.CategoryId = dto.CategoryId ?? product.CategoryId;
        await context.SaveChangesAsync();
        return true;

    }
}