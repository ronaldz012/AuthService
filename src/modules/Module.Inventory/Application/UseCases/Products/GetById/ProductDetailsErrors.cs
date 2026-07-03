using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Products.GetById;

public static class ProductDetailsErrors
{
    public static readonly Error ProductNotFound = new(ErrorCode.NotFound, "Product not found");
}
