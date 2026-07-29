using System.Api.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Common.Services;
using Microsoft.Extensions.Logging;
using Module.Auth.Application.Abstraction;
using Module.Auth.Application.UseCases.Autentication;
using Module.Auth.Application.UseCases.Autentication.Login;
using Module.Auth.Application.UseCases.Autentication.SetupUserPassword;
using Module.Auth.Application.UseCases.Users;

namespace System.Api.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Authentication | Authorization")]
    public class AuthController(
        AutenticationUseCases autenticationUseCases,
        IRefreshTokenService refreshTokenService,
        ILogger<AuthController> logger) : ControllerBase
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
            var result = await autenticationUseCases.Login.Execute(dto);

            if (result.IsSuccess)
                SetAuthCookies(result.Value!.AccessToken, result.Value.RefreshToken);

            return result.ToValueOrProblemDetails();
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
        public async Task<IActionResult> RefreshToken()
        {
            var rawToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(rawToken))
                return Unauthorized();

            var refreshToken = Uri.UnescapeDataString(rawToken);
            logger.LogInformation("RefreshToken cookie raw: {Raw}, decoded: {Decoded}", rawToken, refreshToken);

            var result = await autenticationUseCases.RefreshToken.Execute(refreshToken);

            if (result.IsSuccess)
                SetAuthCookies(result.Value.AccessToken, result.Value.RefreshToken);

            return result.ToValueOrProblemDetails();
        }

        private void SetAuthCookies(string accessToken, string refreshToken)
        {
            var accessOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                MaxAge = TimeSpan.FromMinutes(60),
            };
            accessOptions.Extensions.Add("Partitioned");
            Response.Cookies.Append("accessToken", accessToken, accessOptions);

            var refreshOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                MaxAge = TimeSpan.FromDays(30),
            };
            refreshOptions.Extensions.Add("Partitioned");
            Response.Cookies.Append("refreshToken", refreshToken, refreshOptions);
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            var rawToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(rawToken))
            {
                var refreshToken = Uri.UnescapeDataString(rawToken);
                await refreshTokenService.RevokeAsync(refreshToken);
            }

            Response.Cookies.Delete("accessToken", new CookieOptions { Path = "/" });
            Response.Cookies.Delete("refreshToken", new CookieOptions { Path = "/" });

            return Ok();
        }
    }
}