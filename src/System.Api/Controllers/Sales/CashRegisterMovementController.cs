using System.Api.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Sales.Application.UseCases;
using Module.Sales.Application.UseCases.Movements.Create;
using Module.Sales.Application.UseCases.Movements.Update;

namespace System.Api.Controllers.Sales;

[Route("api/[controller]")]
[ApiController]
[Tags("Sales | CashRegisterMovements")]
[Authorize]
public class CashRegisterMovementController(MovementUseCases movementUseCases) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMovementDto dto)
    {
        return await movementUseCases.CreateMovement.Execute(dto).ToValueOrProblemDetails();
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        return await movementUseCases.ListMovements.Execute().ToValueOrProblemDetails();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovementDto dto)
    {
        return await movementUseCases.UpdateMovement.Execute(id, dto).ToValueOrProblemDetails();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        return await movementUseCases.DeleteMovement.Execute(id).ToValueOrProblemDetails();
    }
}
