using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Products.UpdateStatus;

public static class UpdateProductStatusErrors
{
    public static readonly Error ProductNotFound = new(ErrorCode.NotFound, "Product not found");
}