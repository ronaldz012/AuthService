using System.Api.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Module.Auth.Application.Abstraction;
using Module.Auth.Application.UseCases.Autentication;
using Module.Auth.Application.UseCases.Autentication.SetupUserPassword;
using Module.Auth.Application.UseCases.Users;

namespace System.Api.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Authentication | Authorization")]
    public class AuthController(
        AutenticationUseCases authenticationUseCases) : ControllerBase
    {
        [HttpPost("Register/User")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            return await authenticationUseCases.RegisterDefaultUser.Execute(dto)
                                                    .ToValueOrProblemDetails();
        }

        [HttpGet("Me")]
        [Authorize]
        public async Task<IActionResult> AuthMe()
        {
            return await authenticationUseCases.AuthMe.Execute().ToValueOrProblemDetails();
        }

        [HttpPost("VerifyAccount")]
        public async Task<IActionResult> VerifyAccount([FromBody] string code)
        {
            return await authenticationUseCases.VerifyUser.Execute(code)
                                                        .ToValueOrProblemDetails();
        }

        [HttpPost("complete")]
        [AllowAnonymous]
        public async Task<IActionResult> CompleteTenant([FromBody] SetupUserPasswordRequest request)
        {
            return await authenticationUseCases.SetupUserPassword.ExecuteAsync(request).ToValueOrProblemDetails();
        }

        [HttpPost("verify-token")]
        public async Task<IActionResult> VerifyToken([FromBody] string token)
        {
            return await authenticationUseCases.VerifyToken.ExecuteAsync(token).ToValueOrProblemDetails();
        }
    }
}