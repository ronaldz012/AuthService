using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace System.Api.Middlewares;

public class TenantMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITenantConnectionContext tenantConnectionContext, ITenantDatabaseResolver resolver)
    {
        var endpoint = context.GetEndpoint();

        if (endpoint is null || endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            await next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        var tenantIdClaim = context.User.FindFirst("tenantId")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            await WriteProblemDetails(context, StatusCodes.Status400BadRequest,
                ErrorCode.BadRequest, "Invalid token: missing tenantId");
            return;
        }

        var info = await resolver.GetTenantDatabaseInfo(tenantId);
        if (info is null || string.IsNullOrEmpty(info.Schema))
        {
            await WriteProblemDetails(context, StatusCodes.Status404NotFound,
                ErrorCode.NotFound, "Customer environment not found");
            return;
        }

        tenantConnectionContext.TenantId = tenantId;
        tenantConnectionContext.Schema = info.Schema;
        tenantConnectionContext.DatabaseName = info.DatabaseName;

        await next(context);
    }

    private static async Task WriteProblemDetails(HttpContext context, int statusCode, ErrorCode errorCode, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = errorCode.ToString(),
            Detail = detail
        });
    }
}