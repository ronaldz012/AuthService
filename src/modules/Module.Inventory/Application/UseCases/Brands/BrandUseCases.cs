using Module.Inventory.Application.UseCases.Brands.CreateBrand;

namespace Module.Inventory.Application.UseCases.Brands;

public record BrandUseCases(CreateBrandUc CreateBrand, GetBrands.GetBrands GetBrands);
