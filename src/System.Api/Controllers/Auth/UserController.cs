
using System.Api.Result;
using Common.Contracts.authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Auth.Application.Abstraction;
using Module.Auth.Application.UseCases.Users;
using Module.Auth.Application.UseCases.Users.CreateUser;
using Module.Auth.Application.UseCases.Users.CreateTenantAdmin;
using Module.Auth.Application.UseCases.Users.GetAllUsers;
using Module.Auth.Application.UseCases.Users.UpdateUserStatus;
using Module.Auth.Application.UseCases.Users.UpdateUser;
using Module.Auth.Application.UseCases.Users.GetUserDetails;
using System.Api.Attributes;
using Module.Auth.Domain;

namespace System.Api.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Tags("Authentication | Users")]
    public class UserController(UserUserCases userUserCases, ISessionStateService currentUser) : ControllerBase
    {

        [HttpGet]
        [RequireUserType(UserType.TenantAdmin)]
        public async Task<IActionResult> GetUsers([FromQuery]UserQueryDto request)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await userUserCases.GetAllUsers.Execute(actorResult.Value, request).ToValueOrProblemDetails();
        }

        [HttpPost]
        [RequireUserType(UserType.TenantAdmin)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await userUserCases.CreateUser.Execute(actorResult.Value, request).ToValueOrProblemDetails();
        }

        [HttpPost("tenant-admin")]
        [RequireUserType(UserType.TenantAdmin)]
        public async Task<IActionResult> CreateTenantAdmin([FromBody] CreateTenantAdminRequest request)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await userUserCases.CreateTenantAdmin.Execute(actorResult.Value, request).ToValueOrProblemDetails();
        }

        [HttpPatch("{id:guid}/status")]
        [RequireUserType(UserType.TenantAdmin)]
        public async Task<IActionResult> UpdateUserStatus([FromRoute] Guid id)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await userUserCases.UpdateUserStatus.Execute(actorResult.Value, id).ToValueOrProblemDetails();
        }

        [HttpPut("{id:guid}")]
        [RequireUserType(UserType.TenantAdmin)]
        public async Task<IActionResult> UpdateUser([FromRoute] Guid id, [FromBody] UpdateUserRequest request)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await userUserCases.UpdateUser.Execute(actorResult.Value, id, request).ToValueOrProblemDetails();
        }

        [HttpGet("{id:guid}/details")]
        [RequireUserType(UserType.TenantAdmin)]
        public async Task<IActionResult> GetUserDetails([FromRoute] Guid id)
        {
            return await userUserCases.GetUserDetails.Execute(id).ToValueOrProblemDetails();
        }

        [HttpPatch("{id:guid}/type")]
        [RequireUserType(UserType.TenantAdmin)]
        public async Task<IActionResult> ToggleUserType([FromRoute] Guid id)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await userUserCases.ToggleUserType.Execute(actorResult.Value, id).ToValueOrProblemDetails();
        }
    }
}
 