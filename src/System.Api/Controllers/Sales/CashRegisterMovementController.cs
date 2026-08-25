using System.Api.Attributes;
using System.Api.Result;
using Common.Contracts.authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Auth.Application.Abstraction;
using Module.Sales.Application.UseCases;
using Module.Sales.Application.UseCases.Movements.Create;
using Module.Sales.Application.UseCases.Movements.Update;

namespace System.Api.Controllers.Sales;

[Route("api/[controller]")]
[ApiController]
[Tags("Sales | CashRegisterMovements")]
[Authorize]
public class CashRegisterMovementController(MovementUseCases movementUseCases, ISessionStateService currentUser) : ControllerBase
{
    [HttpPost]
    [RequireFeature("pos", "create")]
    public async Task<IActionResult> Create([FromBody] CreateMovementDto dto)
    {
        var actorResult = currentUser.GetActorContext();
        if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
        return await movementUseCases.CreateMovement.Execute(actorResult.Value, dto).ToValueOrProblemDetails();
    }

    [HttpGet]
    [RequireFeature("pos", "read")]
    public async Task<IActionResult> List()
    {
        var actorResult = currentUser.GetActorContext();
        if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
        return await movementUseCases.ListMovements.Execute(actorResult.Value).ToValueOrProblemDetails();
    }

    [HttpPut("{id:guid}")]
    [RequireFeature("pos", "update")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovementDto dto)
    {
        var actorResult = currentUser.GetActorContext();
        if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
        return await movementUseCases.UpdateMovement.Execute(actorResult.Value, id, dto).ToValueOrProblemDetails();
    }

    [HttpDelete("{id:guid}")]
    [RequireFeature("pos", "delete")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var actorResult = currentUser.GetActorContext();
        if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
        return await movementUseCases.DeleteMovement.Execute(actorResult.Value, id).ToValueOrProblemDetails();
    }
}
