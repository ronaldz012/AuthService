using Module.Inventory.Application.UseCases.Categories.Create;
using Module.Inventory.Application.UseCases.Categories.Get;
using Module.Inventory.Application.UseCases.Categories.Update;

namespace Module.Inventory.Application.UseCases.Categories;

public record CategoryUseCases(CreateCategory CreateCategory, GetCategories GetCategories, UpdateCategory UpdateCategory);