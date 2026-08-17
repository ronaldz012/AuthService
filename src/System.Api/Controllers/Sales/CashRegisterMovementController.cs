using System.Api.Attributes;
using System.Api.Result;
using Common.Contracts.authentication;
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
public class CashRegisterMovementController(MovementUseCases movementUseCases, ICurrentUser currentUser) : ControllerBase
{
    [HttpPost]
    [RequireFeature("pos", "create")]
    public async Task<IActionResult> Create([FromBody] CreateMovementDto dto)
    {
        return await movementUseCases.CreateMovement.Execute(currentUser.ToActorContext(), dto).ToValueOrProblemDetails();
    }

    [HttpGet]
    [RequireFeature("pos", "read")]
    public async Task<IActionResult> List()
    {
        return await movementUseCases.ListMovements.Execute(currentUser.ToActorContext()).ToValueOrProblemDetails();
    }

    [HttpPut("{id:guid}")]
    [RequireFeature("pos", "update")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovementDto dto)
    {
        return await movementUseCases.UpdateMovement.Execute(currentUser.ToActorContext(), id, dto).ToValueOrProblemDetails();
    }

    [HttpDelete("{id:guid}")]
    [RequireFeature("pos", "update")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        return await movementUseCases.DeleteMovement.Execute(currentUser.ToActorContext(), id).ToValueOrProblemDetails();
    }
}
