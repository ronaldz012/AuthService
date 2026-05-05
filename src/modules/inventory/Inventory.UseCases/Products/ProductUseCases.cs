using Inventory.UseCases.ProductVariants;

namespace Inventory.UseCases.Products;

public record ProductUseCases(CreateProduct CreateProduct, ListProducts ListProducts, 
    ValidateProducts ValidateProducts, 
    ValidateProductVariants ValidateProductVariants,
    SearchProduct SearchProducts,
    GetProductVariantByCode  GetProductVariantByCode,
    ProductDetails ProductDetails,
    UpdateProduct UpdateProduct,
    DeleteProduct DeleteProduct);