using Module.Sales.Application.UseCases.Movements.Create;
using Module.Sales.Application.UseCases.Movements.Delete;
using Module.Sales.Application.UseCases.Movements.List;
using Module.Sales.Application.UseCases.Movements.Update;

namespace Module.Sales.Application.UseCases;

public record MovementUseCases(
    CreateMovement CreateMovement,
    ListMovements ListMovements,
    UpdateMovement UpdateMovement,
    DeleteMovement DeleteMovement);
