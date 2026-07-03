using Common.Utilities;

namespace Module.Inventory.Application.UseCases.ProductVariants.Update;

public static class UpdateProductVariantErrors
{
    public static readonly Error VariantNotFound = new(ErrorCode.NotFound, "Product variant not found");
}
