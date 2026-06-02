using Inventory.UseCases.ProductVariants;
using Inventory.Contracts.Dtos.ProductVariants;

namespace Inventory.UseCases.Products;

public record ProductUseCases(CreateProductUc CreateProductUc, ListProducts ListProducts, 
    ValidateProducts ValidateProducts, 
    ValidateProductVariants ValidateProductVariants,
    SearchProduct SearchProducts,
    GetProductVariantByCode  GetProductVariantByCode,
    ProductDetails ProductDetails,
    UpdateProduct UpdateProduct,
    DeleteProduct DeleteProduct);