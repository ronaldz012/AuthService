using Inventory.UseCases.ProductVariants;
using Inventory.Contracts.Dtos.ProductVariants;

namespace Inventory.UseCases.Products;

public record ProductUseCases(CreateProductUc CreateProductUc, ListProducts ListProducts, 
    SearchProduct SearchProducts,
    ProductDetails ProductDetails,
    UpdateProduct UpdateProduct,
    DeleteProduct DeleteProduct);