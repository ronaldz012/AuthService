using Common.Utilities;

namespace Module.Inventory.Application.UseCases.ProductVariants.Delete;

public static class DeleteProductVariantErrors
{
    public static readonly Error VariantNotFound = new(ErrorCode.NotFound, "Product variant not found");
    public static readonly Error VariantHasMovements = new(ErrorCode.Conflict, "Cannot delete the product variant because it already has associated stock movements.");
    public static readonly Error VariantHasTransfers = new(ErrorCode.Conflict, "Cannot delete the product variant because it is referenced by a stock transfer.");
}
