using System.Api.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Module.Auth.Application.Abstraction;
using Module.Auth.Application.UseCases.Users;

namespace System.Api.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Authentication | Authorization")]
    public class AuthController(ISessionStateService sessionStateService) : ControllerBase
    {


        [HttpGet("Me")]
        [Authorize]
        public async Task<IActionResult> AuthMe()
        {
            return sessionStateService.GetSessionAsync().ToValueOrProblemDetails();
            
        }
        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            return Ok();
        }

 


    }
}