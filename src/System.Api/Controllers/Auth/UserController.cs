
using System.Api.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Auth.Application.UseCases.Users;
using Module.Auth.Application.UseCases.Users.CreateUser;
using Module.Auth.Application.UseCases.Users.CreateTenantAdmin;
using Module.Auth.Application.UseCases.Users.GetAllUsers;
using Module.Auth.Application.UseCases.Users.UpdateUserStatus;
using Module.Auth.Application.UseCases.Users.UpdateUser;
using Module.Auth.Application.UseCases.Users.GetUserDetails;

namespace System.Api.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Authentication | Users")]
    public class UserController(UserUserCases userUserCases) : ControllerBase
    {

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserQueryDto request)
        {
            return await userUserCases.GetAllUsers.Execute(request).ToValueOrProblemDetails();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            return await userUserCases.CreateUser.Execute(request).ToValueOrProblemDetails();
        }

        [HttpPost("tenant-admin")]
        public async Task<IActionResult> CreateTenantAdmin([FromBody] CreateTenantAdminRequest request)
        {
            return await userUserCases.CreateTenantAdmin.Execute(request).ToValueOrProblemDetails();
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateUserStatus([FromRoute] Guid id)
        {
            return await userUserCases.UpdateUserStatus.Execute(id).ToValueOrProblemDetails();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser([FromRoute] Guid id, [FromBody] UpdateUserRequest request)
        {
            return await userUserCases.UpdateUser.Execute(id, request).ToValueOrProblemDetails();
        }

        [HttpGet("{id:guid}/details")]
        public async Task<IActionResult> GetUserDetails([FromRoute] Guid id)
        {
            return await userUserCases.GetUserDetails.Execute(id).ToValueOrProblemDetails();
        }

        [HttpPatch("{id:guid}/type")]
        public async Task<IActionResult> ToggleUserType([FromRoute] Guid id)
        {
            return await userUserCases.ToggleUserType.Execute(id).ToValueOrProblemDetails();
        }
    }
}
 