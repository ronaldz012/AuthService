using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Categories.Update;

public static class UpdateCategoryErrors
{
    public static readonly Error CategoryNotFound = new(ErrorCode.NotFound, "Category not found");
    public static readonly Error CategoryNameAlreadyExists = new(ErrorCode.Duplicate, "A category with the same name already exists");
}