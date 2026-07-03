using Common.Utilities;

namespace Module.Inventory.Application.UseCases.ProductVariants.GetBySku;

public static class GetProductVariantByCodeErrors
{
    public static readonly Error VariantNotFound = new(ErrorCode.NotFound, "Product variant not found");
}
