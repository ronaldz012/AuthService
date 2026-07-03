using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Products.Update;

public static class UpdateProductErrors
{
    public static readonly Error ProductNotFound = new(ErrorCode.NotFound, "Product not found");
}
