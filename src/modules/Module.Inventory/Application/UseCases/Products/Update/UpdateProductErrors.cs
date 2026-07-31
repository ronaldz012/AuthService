using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Products.Update;

public static class UpdateProductErrors
{
    public static readonly Error ProductNotFound = new(ErrorCode.NotFound, "Product not found");
    public static readonly Error ProductNameAlreadyExists = new(ErrorCode.Conflict, "A product with the same name already exists for this category and brand");
}
