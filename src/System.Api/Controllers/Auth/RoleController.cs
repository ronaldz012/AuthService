using System.Api.Attributes;
using System.Api.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Module.Auth.Application.UseCases.Roles;
using Module.Auth.Application.UseCases.Roles.GetById;
using Module.Auth.Application.UseCases.Roles.Create;
using Module.Auth.Domain;

namespace System.Api.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Authentication | Roles")]
    [Authorize]
    [RequireUserType(UserType.TenantAdmin)]
    public class RoleController(RoleUseCases roleUseCases) : ControllerBase
    {

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateRoleDto dto)
        {
            return await roleUseCases.AddRole.Execute(dto)
                                            .ToValueOrProblemDetails();
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return await roleUseCases.GetRoleById.Execute(id)
                                        .ToValueOrProblemDetails();
        }
        [HttpGet]
        public async Task<IActionResult> List()
        {
            return await roleUseCases.GetRoles.Execute()
                                                .ToValueOrProblemDetails();
        }
    }
}
