using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Products.Create;

public static class CreateProductErrors
{
    public static readonly Error BrandNotFound = new(ErrorCode.NotFound, "Brand not found");
    public static readonly Error ColorsNotFound = new(ErrorCode.NotFound, "One or more colors not found");
    public static readonly Error ProductRetrievalFailed = new(ErrorCode.InternalError, "Error retrieving created product");
}
