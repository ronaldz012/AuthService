using System.Api.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Inventory.Application.UseCases.Providers;
using Module.Inventory.Application.UseCases.Providers.CreateProvider;
using Module.Inventory.Application.UseCases.Providers.UpdateProvider;

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
        public async Task<IActionResult> GetProviders()
        {
            return await useCases.GetProviders.Execute().ToValueOrProblemDetails();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateProvider([FromRoute] Guid id, [FromBody] UpdateProviderRequest dto)
        {
            return await useCases.UpdateProvider.Execute(id, dto).ToValueOrProblemDetails();
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> ToggleProvider([FromRoute] Guid id)
        {
            return await useCases.ToggleProvider.Execute(id).ToValueOrProblemDetails();
        }
    }
}
