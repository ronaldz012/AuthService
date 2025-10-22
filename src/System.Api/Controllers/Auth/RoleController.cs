using System.Api.Result;
using Auth.Dtos.Roles;
using Auth.UseCases.Roles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace System.Api.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController(RoleUseCases roleUseCases) : ControllerBase
    {

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateRoleDto dto)
        {
            return await roleUseCases.AddRole.Execute(dto)
                                            .ToValueOrProblemDetails();
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return await roleUseCases.GetRole.Execute(id)
                                        .ToValueOrProblemDetails();
        }
    }
}
