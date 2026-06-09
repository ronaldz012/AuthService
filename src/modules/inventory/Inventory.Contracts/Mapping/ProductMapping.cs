using Inventory.Contracts.Dtos.ProductVariants;
using Inventory.Data.Entities.Products;

namespace Inventory.Contracts.Dtos.Products;

public static class ProductMappingExtensions
{
    public static void MapFrom(this Product product,UpdateProductDto dto)
    {
        product.Name = dto.Name ?? product.Name;
        product.Description = dto.Description ?? product.Description;
        product.CategoryId = dto.CategoryId ?? product.CategoryId;
        product.Gender = dto.Gender ?? product.Gender;
        
        product.UpdatedAt = DateTime.UtcNow;
    }

    public static void MapTo(this ProductVariant productVariant, UpdateProductVariantDto dto)
    {
    
        productVariant.Description = dto.Description ?? productVariant.Description;
        productVariant.Price = dto.Price ?? productVariant.Price;
    }
    
}