using System.Security.Claims;
using Common.Contracts.authentication;
using Microsoft.Extensions.Options;
using Common.Services;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Infrastructure.Persistence;

namespace System.Api.Middlewares;

public class TenantMiddleware(RequestDelegate next) 
{
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, AuthDbContext authDbContext)
    {
        var path = context.Request.Path;
        if (path.StartsWithSegments("/api/system") || 
            path.StartsWithSegments("/scalar") || 
            path.StartsWithSegments("/openapi")) 
        {        
            await next(context);
            return;
        }

        string? schema = null;
        Guid? tenantId = null;
        string? databaseName = null;

        if (context.User.Identity?.IsAuthenticated == true)
        {
            schema       = context.User.FindFirstValue("schema");
            databaseName = context.User.FindFirstValue("databaseName"); // null si usa DB default
            
            var tidClaim = context.User.FindFirstValue("tenantId");
            if (Guid.TryParse(tidClaim, out var guid))
                tenantId = guid;
        }
        else
        {
            var host = context.Request.Headers["X-Forwarded-Host"].ToString().Split('.')[0];
            if (string.IsNullOrEmpty(host)) 
                host = context.Request.Host.Host.Split('.')[0];

            await next(context);
            return;
        }

        if (tenantId == null || string.IsNullOrEmpty(schema))
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { error = "Ambiente de cliente no encontrado" });
            return;
        }

        tenantContext.Schema       = schema;
        tenantContext.TenantId     = tenantId;
        tenantContext.DatabaseName = databaseName; // null = usa DefaultConnection tal cual

        await next(context);
    }
}