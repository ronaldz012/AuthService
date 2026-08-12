using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Sizes.Create;

public static class CreateSizeErrors
{
    public static readonly Error SizeAlreadyExists = new(ErrorCode.Duplicate, "Size already exists");
}