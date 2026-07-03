using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Products.Delete;

public static class DeleteProductErrors
{
    public static readonly Error InventoryStillAvailable = new(ErrorCode.InvalidState, "Inventory still available");
    public static readonly Error ProductNotFound = new(ErrorCode.NotFound, "Product not found");
}
