using System.Security.Claims;
using Microsoft.Extensions.Options;
using Common.Data;
using Common.Services;
using Microsoft.EntityFrameworkCore;
using shared.Module.Data;

namespace System.Api.Middlewares;

public class TenantMiddleware(RequestDelegate next) 
{
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext,SharedDbContext sharedDbContext)
    {
        if (context.Request.Path.StartsWithSegments("/api/system"))
        {
            await next(context);
            return;
        }

        string? schema = null;
        Guid? tenantId = null;

        if (context.User.Identity?.IsAuthenticated == true)
        {
            schema = context.User.FindFirstValue("schema"); // O el nombre del claim que elijas
            var tidClaim = context.User.FindFirstValue("tenantId");
            
            if (Guid.TryParse(tidClaim, out var guid))
                tenantId = guid;
        }
        else
        {
            // Ejemplo: cliente.tuapp.com -> "cliente"
            var host = context.Request.Headers["X-Forwarded-Host"].ToString().Split('.')[0];
            if (string.IsNullOrEmpty(host)) host = context.Request.Host.Host.Split('.')[0];

            // Buscamos en la base de datos compartida por el identificador (slug/host)
            var tenant = await sharedDbContext.Tenants
                .Where(t => t.IsActive)
                .FirstOrDefaultAsync(x => x.DisplayName.ToLower() == host.ToLower());

            if (tenant != null)
            {
                schema = tenant.Schema; // O el campo donde guardes el esquema
                tenantId = tenant.Id;
            }
        }

        // 4. Validación Final
        if (tenantId == null || string.IsNullOrEmpty(schema))
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { error = "Ambiente de cliente no encontrado" });
            return;
        }

        // 5. Asignación al contexto inyectado
        tenantContext.Schema = schema;
        tenantContext.TenantId = tenantId;

        await next(context);
    }
}