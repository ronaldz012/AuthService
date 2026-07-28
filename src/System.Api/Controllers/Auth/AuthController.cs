using System.Api.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Common.Services;
using Module.Auth.Application.UseCases.Autentication;
using Module.Auth.Application.UseCases.Autentication.Login;
using Module.Auth.Application.UseCases.Autentication.RefreshToken;
using Module.Auth.Application.UseCases.Autentication.SetupUserPassword;
using Module.Auth.Application.UseCases.Users;

namespace System.Api.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Authentication | Authorization")]
    public class AuthController(AutenticationUseCases autenticationUseCases) : ControllerBase
    {
        [HttpPost("Register/User")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            return await autenticationUseCases.RegisterDefaultUser.Execute(dto)
                                                    .ToValueOrProblemDetails();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest dto)
        {
            return await autenticationUseCases.Login.Execute(dto)
                                                    .ToValueOrProblemDetails();
        }

        [HttpGet("Me")]
        [Authorize]
        public async Task<IActionResult> AuthMe()
        {
            return await autenticationUseCases.AuthMe.Execute().ToValueOrProblemDetails();
        }

        [HttpPost("VerifyAccount")]
        public async Task<IActionResult> VerifyAccount([FromBody] string code)
        {
            return await autenticationUseCases.VerifyUser.Execute(code)
                                                        .ToValueOrProblemDetails();
        }

        [HttpPost("CompleteUser")]
        [Authorize]
        public async Task<IActionResult> CompleteUserRole([FromBody] CompleteUserRoleDto dto)
        {
            return await autenticationUseCases.CompletePublicRegister.Execute(dto).ToValueOrProblemDetails();
        }

        [HttpPost("complete")]
        [AllowAnonymous]
        public async Task<IActionResult> CompleteTenant([FromBody] SetupUserPasswordRequest request)
        {
            return await autenticationUseCases.SetupUserPassword.ExecuteAsync(request).ToValueOrProblemDetails();
        }

        [HttpPost("verify-token")]
        public async Task<IActionResult> VerifyToken([FromBody] string token)
        {
            return await autenticationUseCases.VerifyToken.ExecuteAsync(token).ToValueOrProblemDetails();
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            return await autenticationUseCases.RefreshToken.Execute(request.RefreshToken).ToValueOrProblemDetails();
        }
    }
}