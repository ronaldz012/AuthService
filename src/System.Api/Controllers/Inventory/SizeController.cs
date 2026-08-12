using System.Api.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Inventory.Application.UseCases.Sizes;
using Module.Inventory.Application.UseCases.Sizes.Create;

namespace System.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Inventory | Sizes")]
    [Authorize]
    public class SizeController(SizeUseCases useCases) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return await useCases.GetListSizes.Execute().ToValueOrProblemDetails();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSizeDto dto)
        {
            return await useCases.CreateSize.Execute(dto).ToValueOrProblemDetails();
        }
    }
}