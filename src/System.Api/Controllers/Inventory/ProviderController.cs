using System.Api.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Inventory.Application.UseCases.Providers;
using Module.Inventory.Application.UseCases.Providers.CreateProvider;
using Module.Inventory.Application.UseCases.Providers.Update;

namespace System.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Inventory | Providers")]
    [Authorize]
    public class ProviderController(ProviderUseCases useCases) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateProvider([FromBody] CreateProviderRequest dto)
        {
            return await useCases.CreateProvider.Execute(dto).ToValueOrProblemDetails();
        }

        [HttpGet]
        public async Task<IActionResult> GetProviders([FromQuery] bool? includeInactive)
        {
            return await useCases.GetProviders.Execute(includeInactive).ToValueOrProblemDetails();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateProvider([FromRoute] Guid id, [FromBody] UpdateProviderRequest dto)
        {
            return await useCases.UpdateProvider.Execute(id, dto).ToValueOrProblemDetails();
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> ToggleProvider([FromRoute] Guid id)
        {
            return await useCases.UpdateProvider.ChangeStatus(id).ToValueOrProblemDetails();
        }
    }
}
