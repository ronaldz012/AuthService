namespace Inventory.UseCases.Products;

public record ProductUseCases(CreateProduct CreateProduct, ListProducts ListProducts, 
    ValidateProducts ValidateProducts, 
    ValidateProductVariants ValidateProductVariants,
    SearchProduct SearchProducts,
    ProductDetails ProductDetails);