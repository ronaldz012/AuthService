using Module.Inventory.Application.UseCases.Products.Create;
using Module.Inventory.Application.UseCases.Products.Delete;
using Module.Inventory.Application.UseCases.Products.Get;
using Module.Inventory.Application.UseCases.Products.GetById;
using Module.Inventory.Application.UseCases.Products.Search;
using Module.Inventory.Application.UseCases.Products.Update;
using Module.Inventory.Application.UseCases.Products.UpdateStatus;

namespace Module.Inventory.Application.UseCases.Products;

public record ProductUseCases(CreateProductUc CreateProductUc, GetProductsUc GetProductsUc, 
    SearchProduct SearchProducts,
    ProductDetails ProductDetails,
    UpdateProduct UpdateProduct,
    DeleteProduct DeleteProduct,
    UpdateProductStatus UpdateProductStatus);