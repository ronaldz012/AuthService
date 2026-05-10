using System.Api.Result;
using Inventory.Contracts.Dtos;
using Inventory.UseCases.Colors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace System.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ColorController(ColoreUseCases useCases) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] ColoreQueryDto dto)
        {
            return await useCases.getListColors.Execute(dto).ToValueOrProblemDetails();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateColorDto dto)
        {
            return await useCases.createColor.Execute(dto).ToValueOrProblemDetails();
        }
    }
}
