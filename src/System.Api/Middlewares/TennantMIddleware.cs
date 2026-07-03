using System.Security.Claims;
using Common.Contracts.authentication;

namespace System.Api.Middlewares;

public class TenantMiddleware(RequestDelegate next) 
{
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, ITenantDatabaseResolver resolver)
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
            var tidClaim = context.User.FindFirstValue("tenantId");
            if (Guid.TryParse(tidClaim, out var guid))
                tenantId = guid;

            if (tenantId is not null)
            {
                var info = await resolver.GetTenantDatabaseInfo(tenantId.Value);
                if (info is not null)
                {
                    schema       = info.Schema;
                    databaseName = info.DatabaseName;
                }
            }
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
        tenantContext.DatabaseName = databaseName;

        await next(context);
    }
}