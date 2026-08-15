using System.Api.Result;
using Common.Contracts.authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Inventory.Application.UseCases.Receptions;
using Module.Inventory.Application.UseCases.Receptions.Create;
using Module.Inventory.Application.UseCases.Receptions.Get;

namespace System.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Inventory | Receptions")]
    [Authorize]
    public class ReceptionController(ReceptionUseCases service, ICurrentUser currentUser) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateReception(CreateStockReceptionDto dto)
        {
            return await service.CreateReceptionUc.Execute(currentUser.ToActorContext(), dto).ToValueOrProblemDetails();
        }

        [HttpGet]
        public async Task<IActionResult> ListReceptions([FromQuery] ReceptionQueryDto dto)
        {
            return await service.ListReceptions.Execute(currentUser.ToActorContext(), dto).ToValueOrProblemDetails();
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetReception([FromRoute] Guid id)
        {
            return await service.GetReception.Execute(currentUser.ToActorContext(), id).ToValueOrProblemDetails();
        }

        [HttpGet("{id:guid}/labels")]
        public async Task<IActionResult> GetLabels([FromRoute] Guid id)
        {
            return await service.ReceptionLabels.Execute(id).ToValueOrProblemDetails();
        }

        [HttpGet("{id:guid}/can-revert")]
        public async Task<IActionResult> CheckCanRevert([FromRoute] Guid id)
        {
            return await service.RevertStockReception.Check(currentUser.ToActorContext(), id).ToValueOrProblemDetails();
        }

        [HttpPost("{id:guid}/revert")]
        public async Task<IActionResult> Revert([FromRoute] Guid id)
        {
            return await service.RevertStockReception.Execute(currentUser.ToActorContext(), id).ToValueOrProblemDetails();
        }
    }
}
