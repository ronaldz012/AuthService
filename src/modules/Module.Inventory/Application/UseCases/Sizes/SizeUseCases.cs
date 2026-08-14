using Module.Inventory.Application.UseCases.Sizes.Create;
using Module.Inventory.Application.UseCases.Sizes.List;
using Module.Inventory.Application.UseCases.Sizes.Update;

namespace Module.Inventory.Application.UseCases.Sizes;

public record SizeUseCases(CreateSize CreateSize, GetListSizes GetListSizes, UpdateSize UpdateSize);