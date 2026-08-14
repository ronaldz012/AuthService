using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Colors.Update;

public static class UpdateColorErrors
{
    public static readonly Error ColorNotFound = new(ErrorCode.NotFound, "Color not found");
    public static readonly Error ColorNameAlreadyExists = new(ErrorCode.Duplicate, "A color with the same name already exists");
}