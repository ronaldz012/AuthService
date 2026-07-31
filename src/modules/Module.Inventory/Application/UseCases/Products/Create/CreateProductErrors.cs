using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Products.Create;

public static class CreateProductErrors
{
    public static readonly Error BrandNotFound = new(ErrorCode.NotFound, "Brand not found");
    public static readonly Error CategoryNotFound = new(ErrorCode.NotFound, "Category not found");
    public static readonly Error ProductNameAlreadyExists = new(ErrorCode.Conflict, "A product with the same name already exists for this category and brand");
    public static readonly Error ColorsNotFound = new(ErrorCode.NotFound, "One or more colors not found");
    public static readonly Error ProductRetrievalFailed = new(ErrorCode.InternalError, "Error retrieving created product");
}
