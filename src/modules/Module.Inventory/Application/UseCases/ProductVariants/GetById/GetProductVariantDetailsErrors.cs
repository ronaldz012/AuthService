using Common.Utilities;

namespace Module.Inventory.Application.UseCases.ProductVariants.GetById;

public static class GetProductVariantDetailsErrors
{
    public static readonly Error VariantNotFound = new(ErrorCode.NotFound, "Product variant not found");
}
