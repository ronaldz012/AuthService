using System.Api.Attributes;
using System.Api.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Auth.Application.Abstraction;
using Module.Inventory.Application.UseCases.Providers;
using Module.Inventory.Application.UseCases.Providers.CreateProvider;
using Module.Inventory.Application.UseCases.Providers.Update;

namespace System.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Inventory | Providers")]
    [Authorize]
    public class ProviderController(ProviderUseCases useCases, ISessionStateService currentUser) : ControllerBase
    {
        [HttpPost]
        [RequireFeature("receptions", "create")]
        public async Task<IActionResult> CreateProvider([FromBody] CreateProviderRequest dto)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await useCases.CreateProvider.Execute(actorResult.Value, dto).ToValueOrProblemDetails();
        }

        [HttpGet]
        [RequireFeature("receptions", "read")]
        public async Task<IActionResult> GetProviders([FromQuery] bool? includeInactive)
        {
            return await useCases.GetProviders.Execute(includeInactive).ToValueOrProblemDetails();
        }

        [HttpPut("{id:guid}")]
        [RequireFeature("receptions", "update")]
        public async Task<IActionResult> UpdateProvider([FromRoute] Guid id, [FromBody] UpdateProviderRequest dto)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await useCases.UpdateProvider.Execute(actorResult.Value, id, dto).ToValueOrProblemDetails();
        }

        [HttpPatch("{id:guid}/status")]
        [RequireFeature("receptions", "delete")]
        public async Task<IActionResult> ToggleProvider([FromRoute] Guid id)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await useCases.UpdateProvider.ChangeStatus(actorResult.Value, id).ToValueOrProblemDetails();
        }
    }
}
