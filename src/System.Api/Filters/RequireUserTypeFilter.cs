using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Module.Auth.Application.Abstraction;

namespace System.Api.Filters;

public class RequireUserTypeFilter(
    int requiredUserType,
    ISessionStateService sessionState) : IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var sessionResult = sessionState.GetSessionAsync();
        if (!sessionResult.IsSuccess)
        {
            context.Result = new ObjectResult(new
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "No se pudo autenticar la sesion."
            }) { StatusCode = StatusCodes.Status401Unauthorized };
            return Task.CompletedTask;
        }

        var actualType = sessionResult.Value.User.UserType;

        bool authorized = requiredUserType switch
        {
            2 => actualType == 2, // Owner: exact
            1 => actualType >= 1, // TenantAdmin or Owner
            _ => actualType >= requiredUserType // Standard: any authenticated user
        };

        if (!authorized)
        {
            context.Result = new ObjectResult(new
            {
                StatusCode = StatusCodes.Status403Forbidden,
                Message = "No tiene permisos suficientes para esta accion.",
                Details = $"Se requiere nivel de usuario {(Module.Auth.Domain.UserType)requiredUserType} o superior."
            }) { StatusCode = StatusCodes.Status403Forbidden };
        }

        return Task.CompletedTask;
    }
}
