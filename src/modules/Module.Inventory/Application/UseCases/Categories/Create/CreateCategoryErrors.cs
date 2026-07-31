using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Categories.Create;

public static class CreateCategoryErrors
{
    public static readonly Error CategoryAlreadyExists = new(ErrorCode.Duplicate, "Category already exists");
}
