using Module.Inventory.Application.UseCases.Colors.Create;
using Module.Inventory.Application.UseCases.Colors.List;

namespace Module.Inventory.Application.UseCases.Colors;

public record ColoreUseCases(CreateColor createColor, GetListColors getListColors);