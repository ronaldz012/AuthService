using Module.Inventory.Application.UseCases.Brands.CreateBrand;
using Module.Inventory.Application.UseCases.Brands.Update;

namespace Module.Inventory.Application.UseCases.Brands;

public record BrandUseCases(CreateBrandUc CreateBrand, GetBrands.GetBrands GetBrands, UpdateBrand UpdateBrand);
