using System.Api.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Module.Inventory.Application.UseCases.Colors;
using Module.Inventory.Application.UseCases.Colors.Create;
using Module.Inventory.Application.UseCases.Colors.List;

namespace System.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ColorController(ColoreUseCases useCases) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return await useCases.getListColors.Execute().ToValueOrProblemDetails();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateColorDto colorNameDto)
        {
            return await useCases.createColor.Execute(colorNameDto.Name).ToValueOrProblemDetails();
        }
    }
}
