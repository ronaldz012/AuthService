using System.Api.Result;
using Common.Contracts.authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Inventory.Application.UseCases.Sizes;
using Module.Inventory.Application.UseCases.Sizes.Create;
using Module.Inventory.Application.UseCases.Sizes.Update;

namespace System.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Inventory | Sizes")]
    [Authorize]
    public class SizeController(SizeUseCases useCases, ICurrentUser currentUser) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] bool? includeInactive)
        {
            return await useCases.GetListSizes.Execute(includeInactive).ToValueOrProblemDetails();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSizeDto dto)
        {
            return await useCases.CreateSize.Execute(currentUser.ToActorContext(), dto).ToValueOrProblemDetails();
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> ToggleSizeStatus([FromRoute] Guid id)
        {
            return await useCases.UpdateSize.ChangeStatus(currentUser.ToActorContext(), id).ToValueOrProblemDetails();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateSize([FromRoute] Guid id, [FromBody] UpdateSizeDto dto)
        {
            return await useCases.UpdateSize.Execute(currentUser.ToActorContext(), id, dto).ToValueOrProblemDetails();
        }
    }
}