using Common.Contracts.authentication;
using Common.Contracts.authentication.dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace System.Api.Filters;

public class RequireFeatureFilter(
    string feature,
    string permission,
    bool multiBranch,
    ICurrentUser currentUser,
    ITenantConnectionContext tenantConnectionContext,
    ISessionStateService sessionState, 
    ILogger<RequireFeatureFilter> logger) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (currentUser.IsAdmin) return;

        if (!multiBranch && currentUser.BranchIds.Count > 1)
        {
            context.Result = new ObjectResult(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Este endpoint solo acepta una sucursal activa.",
                Details = "Envíe un único ID en el header 'X-Branch-Id'."
            }) { StatusCode = StatusCodes.Status400BadRequest };
            return;
        }

        if (!currentUser.BranchIds.Any())
        {
            context.Result = new ObjectResult(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "No se especificó ninguna sucursal.",
                Details = "Debe enviar el header 'X-Branch-Id' con un ID válido."
            }) { StatusCode = StatusCodes.Status400BadRequest };
            return;
        }

        var session = await sessionState.GetOrBuildAsync(
            currentUser.UserId, tenantConnectionContext.TenantId!.Value, (UserType)currentUser.UserType);

        var requestedBranches = session.Branches
            .Where(b => currentUser.BranchIds.Contains(b.BranchId))
            .ToList();

        if (requestedBranches.Count != currentUser.BranchIds.Count)
        {
            context.Result = new ObjectResult(new
            {
                StatusCode = StatusCodes.Status403Forbidden,
                Message = "Acceso denegado a una o más sucursales solicitadas."
            }) { StatusCode = StatusCodes.Status403Forbidden };
            return;
        }

        bool hasPermission = requestedBranches.All(branch =>
            branch.Modules.Any(module =>
                module.Features.Any(f =>
                    f.key == feature &&
                    f.Permission.Contains(permission))));

        if (!hasPermission)
        {
            logger.LogWarning(
                "Usuario {UserId} fue rechazado. Falta el permiso '{Permission}' en la Feature '{Feature}' para las Branches: {Branches}",
                currentUser.UserId, permission, feature, string.Join(", ", currentUser.BranchIds));

            context.Result = new ObjectResult(new
            {
                StatusCode = StatusCodes.Status403Forbidden,
                Message = $"No tiene el permiso necesario para realizar esta acción.",
                Details = $"Se requiere la acción '{permission}' en la sección '{feature}'."
            }) { StatusCode = StatusCodes.Status403Forbidden };
        }
    }
}