using Common.Utilities;

namespace Module.Inventory.Application.UseCases.ProductVariants.PatchStock;

public static class CorrectProductVariantStockErrors
{
    public static readonly Error VariantNotFound = new(ErrorCode.NotFound, "Product variant not found");
    public static readonly Error StockCorrectionFailed = new(ErrorCode.ValidationError, "Stock correction failed");
    public static readonly Error SurplusNotAllowed = new(ErrorCode.ValidationError, "Stock increase is not allowed via correction. Use a reception to add stock");
}
