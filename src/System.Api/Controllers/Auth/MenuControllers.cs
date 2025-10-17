using System.Api.Result;
using Auth.Dtos.Modules;
using Auth.UseCases;
using Microsoft.AspNetCore.Mvc;


namespace System.Api.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuControllers(MenuUseCases menuUseCases) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateMenuDto dto)
        {
            return await menuUseCases.AddMenu.Execute(dto)
                                            .ToValueOrProblemDetails();
        }
    }
}
