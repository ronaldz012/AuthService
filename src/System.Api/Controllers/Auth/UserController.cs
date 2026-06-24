
using System.Api.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Auth.Application.UseCases.Users;
using Module.Auth.Application.UseCases.Users.CreateUser;
using Module.Auth.Application.UseCases.Users.GetAllUsers;

namespace System.Api.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Authentication | Users")]
    public class UserController(UserUserCases userUserCases) : ControllerBase
    {

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery]UserQueryDto request)
        {
            return await userUserCases.GetAllUsers.execute(request).ToValueOrProblemDetails();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            return await userUserCases.CreateUser.Execute(request).ToValueOrProblemDetails();
        }
    }
}
 