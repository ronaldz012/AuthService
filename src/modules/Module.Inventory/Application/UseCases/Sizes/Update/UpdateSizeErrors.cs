using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Sizes.Update;

public static class UpdateSizeErrors
{
    public static readonly Error SizeNotFound = new(ErrorCode.NotFound, "Size not found");
    public static readonly Error SizeNameAlreadyExists = new(ErrorCode.Duplicate, "A size with the same name already exists");
}