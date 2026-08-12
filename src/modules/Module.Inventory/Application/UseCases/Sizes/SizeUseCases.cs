using Module.Inventory.Application.UseCases.Sizes.Create;
using Module.Inventory.Application.UseCases.Sizes.List;

namespace Module.Inventory.Application.UseCases.Sizes;

public record SizeUseCases(CreateSize CreateSize, GetListSizes GetListSizes);