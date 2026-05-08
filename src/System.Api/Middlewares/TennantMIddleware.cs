using System.Security.Claims;
using Microsoft.Extensions.Options;
using Common.Data;
using Common.Services;

namespace System.Api.Middlewares;

public class TenantMiddleware(RequestDelegate next, IWebHostEnvironment env,IOptions<TenantOptions> tenantOptions) 
{
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.Request.Path.StartsWithSegments("/api/system"))
        {
            await next(context);
            return;
        }

        string? schema;

        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Autenticado → del JWT, nunca debería fallar
            schema = context.User.FindFirstValue("tenant");
        }
        else
        {
            var host = env.IsDevelopment()
                ? context.Request.Headers["X-Tenant"].ToString()
                : context.Request.Headers["X-Forwarded-Host"].ToString().Split('.')[0];

            schema = host?.ToLower();
            
            if (!tenantOptions.Value.Schemas.Contains(schema?? ""))
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsJsonAsync(new { error = $"Tenant '{schema}' not found" });
                return;
            }
        }

        if (string.IsNullOrEmpty(schema))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "Tenant not found" });
            return;
        }

        tenantContext.Schema = schema;
        await next(context);
    }
}