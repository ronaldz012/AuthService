using Common.Utilities;

namespace Module.Inventory.Application.UseCases.ProductVariants.Create;

public static class CreateProductVariantErrors
{
    public static readonly Error EmptyVariantList = new(ErrorCode.BadRequest, "The variant list cannot be empty.");
    public static readonly Error ProductNotFound = new(ErrorCode.NotFound, "Product not found.");
    public static readonly Error ColorIdsNotFound = new(ErrorCode.NotFound, "One or more color IDs do not exist.");
    public static readonly Error VariantAlreadyExists = new(ErrorCode.Duplicate, "One or more variants with the same size and color already exist for this product.");
}
