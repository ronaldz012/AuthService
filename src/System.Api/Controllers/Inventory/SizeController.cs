using System.Api.Attributes;
using System.Api.Result;
using Common.Contracts.authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Auth.Application.Abstraction;
using Module.Inventory.Application.UseCases.Sizes;
using Module.Inventory.Application.UseCases.Sizes.Create;
using Module.Inventory.Application.UseCases.Sizes.Update;

namespace System.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Inventory | Sizes")]
    [Authorize]
    public class SizeController(SizeUseCases useCases, ISessionStateService currentUser) : ControllerBase
    {
        [HttpGet]
        [RequireFeature("products", "read")]
        public async Task<IActionResult> Get([FromQuery] bool? includeInactive)
        {
            return await useCases.GetListSizes.Execute(includeInactive).ToValueOrProblemDetails();
        }

        [HttpPost]
        [RequireFeature("products", "create")]
        public async Task<IActionResult> Create([FromBody] CreateSizeDto dto)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await useCases.CreateSize.Execute(actorResult.Value, dto).ToValueOrProblemDetails();
        }

        [HttpPatch("{id:guid}/status")]
        [RequireFeature("products", "update")]
        public async Task<IActionResult> ToggleSizeStatus([FromRoute] Guid id)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await useCases.UpdateSize.ChangeStatus(actorResult.Value, id).ToValueOrProblemDetails();
        }

        [HttpPut("{id:guid}")]
        [RequireFeature("products", "update")]
        public async Task<IActionResult> UpdateSize([FromRoute] Guid id, [FromBody] UpdateSizeDto dto)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await useCases.UpdateSize.Execute(actorResult.Value, id, dto).ToValueOrProblemDetails();
        }
    }
}