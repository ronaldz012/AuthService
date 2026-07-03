using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Colors.Create;

public static class CreateColorErrors
{
    public static readonly Error ColorAlreadyExists = new(ErrorCode.Duplicate, "Color already exists");
}
