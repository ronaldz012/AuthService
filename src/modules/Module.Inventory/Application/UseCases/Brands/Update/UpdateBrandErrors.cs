using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Brands.Update;

public static class UpdateBrandErrors
{
    public static readonly Error BrandNotFound = new(ErrorCode.NotFound, "Brand not found");
    public static readonly Error BrandNameAlreadyExists = new(ErrorCode.Duplicate, "A brand with the same name already exists");
}