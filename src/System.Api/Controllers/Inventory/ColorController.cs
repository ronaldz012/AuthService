using System.Api.Attributes;
using System.Api.Result;
using Common.Contracts.authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Module.Auth.Application.Abstraction;
using Module.Inventory.Application.UseCases.Colors;
using Module.Inventory.Application.UseCases.Colors.Create;
using Module.Inventory.Application.UseCases.Colors.List;
using Module.Inventory.Application.UseCases.Colors.Update;

namespace System.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ColorController(ColoreUseCases useCases, ISessionStateService currentUser) : ControllerBase
    {
        [HttpGet]
        [RequireFeature("products", "read")]
        public async Task<IActionResult> Get([FromQuery] bool? includeInactive)
        {
            return await useCases.getListColors.Execute(includeInactive).ToValueOrProblemDetails();
        }

        [HttpPost]
        [RequireFeature("products", "create")]
        public async Task<IActionResult> Create([FromBody] CreateColorDto colorNameDto)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await useCases.createColor.Execute(actorResult.Value, colorNameDto.Name).ToValueOrProblemDetails();
        }

        [HttpPatch("{id:guid}/status")]
        [RequireFeature("products", "update")]
        public async Task<IActionResult> ToggleColorStatus([FromRoute] Guid id)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await useCases.UpdateColor.ChangeStatus(actorResult.Value, id).ToValueOrProblemDetails();
        }

        [HttpPut("{id:guid}")]
        [RequireFeature("products", "update")]
        public async Task<IActionResult> UpdateColor([FromRoute] Guid id, [FromBody] UpdateColorDto dto)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await useCases.UpdateColor.Execute(actorResult.Value, id, dto).ToValueOrProblemDetails();
        }
    }
}
