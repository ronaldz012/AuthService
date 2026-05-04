using Inventory.Contracts.Dtos.Products;
using Inventory.Data.Entities.Products;
using Inventory.Data.Persistence;
using Shared.Result;

namespace Inventory.UseCases.Products;

public class UpdateProduct(InvDbContext context)
{
    public async Task<Result<bool>> Execute(UpdateProductDto dto, int id)
    {
        var product = await context.Products.FindAsync(id);
        if(product == null) return 
            new Error("NOT_FOUND", "Product not found");
        
        product.MapFrom(dto);
        await context.SaveChangesAsync();
        return true;

    }
}