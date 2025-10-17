
namespace System.Api.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Extensions;
using Shared.Result;

/// <summary>
/// Middleware que captura todas las excepciones no manejadas y las convierte en ProblemDetails.
/// Centraliza el manejo de errores para toda la API.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error no manejado en {Path}. Usuario: {User}", 
                context.Request.Path,
                context.User?.Identity?.Name ?? "Anónimo");

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        // Convertir la excepción a Error usando nuestra extensión centralizada
        var error = ex.ToError();
        var statusCode = MapErrorCodeToStatusCode(error.Code);
        
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Code,
            Detail = error.Message,
            Instance = context.Request.Path
        };

        // Agregar información adicional en desarrollo
        if (context.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() == true)
        {
            problemDetails.Extensions["exception"] = ex.GetType().Name;
            problemDetails.Extensions["stackTrace"] = ex.StackTrace;
        }

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    /// <summary>
    /// Mapea códigos de error personalizados a códigos de estado HTTP.
    /// </summary>
    private static int MapErrorCodeToStatusCode(string errorCode)
    {
        return errorCode switch
        {
            // Validación
            "VALIDATION_ERROR" => StatusCodes.Status400BadRequest,
            
            // Base de datos
            "INVALID_REFERENCE" => StatusCodes.Status400BadRequest,
            "INVALID_DATA" => StatusCodes.Status400BadRequest,
            "DATABASE_ERROR" => StatusCodes.Status500InternalServerError,
            "DATABASE_TIMEOUT" => StatusCodes.Status504GatewayTimeout,
            "DATABASE_CONNECTION" => StatusCodes.Status503ServiceUnavailable,
            
            // Duplicados
            "DUPLICATE" => StatusCodes.Status409Conflict,
            
            // No encontrado
            "NOT_FOUND" => StatusCodes.Status404NotFound,
            
            // Autenticación/Autorización
            "UNAUTHORIZED" => StatusCodes.Status401Unauthorized,
            "FORBIDDEN" => StatusCodes.Status403Forbidden,
            
            // Operaciones
            "INVALID_OPERATION" => StatusCodes.Status400BadRequest,
            "TIMEOUT" => StatusCodes.Status408RequestTimeout,
            "CANCELLED" => StatusCodes.Status499ClientClosedRequest,
            
            // Genérico
            "UNEXPECTED_ERROR" => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}