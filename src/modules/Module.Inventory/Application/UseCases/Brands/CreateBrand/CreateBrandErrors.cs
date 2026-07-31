using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Brands.CreateBrand;

public static class CreateBrandErrors
{
    public static readonly Error BrandPrefixAlreadyExists = new(ErrorCode.Duplicate, "There is already a brand with that prefix");
    public static readonly Error BrandNameAlreadyExists = new(ErrorCode.Duplicate, "There is already a brand with that name");
}
