using System;

namespace System.Api.Result;


using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Result;

/// <summary>
/// Extensiones para convertir Result{T} en respuestas HTTP de ASP.NET Core
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Convierte un Result{TValue} en una respuesta HTTP apropiada:
    /// - Si IsSuccess = true: retorna 200 OK con el Value
    /// - Si IsSuccess = false: retorna ProblemDetails con el código de estado según el Error.Code
    /// </summary>
    /// <typeparam name="TValue">Tipo del valor de retorno exitoso</typeparam>
    /// <param name="resultTask">Task que contiene el Result a evaluar</param>
    /// <returns>IActionResult con la respuesta HTTP apropiada</returns>
    public static async Task<IActionResult> ToValueOrProblemDetails<TValue>(
        this Task<Result<TValue>> resultTask)
    {
        // Esperamos el resultado de la tarea
        var result = await resultTask;
        
        // Si fue exitoso, retornamos 200 OK con el valor
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }

        // Si falló, convertimos el Error en ProblemDetails (RFC 7807)
        var statusCode = MapErrorCodeToStatusCode(result.Error!.Code);
        
        return new ObjectResult(new ProblemDetails
        {
            Status = statusCode,
            Title = result.Error.Code,           // Código del error
            Detail = result.Error.Message        // Mensaje descriptivo
        })
        {
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Mapea códigos de error personalizados a códigos de estado HTTP.
    /// Puedes extender este switch con tus propios códigos.
    /// </summary>
    /// <param name="errorCode">Código del error (ej: "VALIDATION_ERROR")</param>
    /// <returns>Código de estado HTTP apropiado</returns>
    private static int MapErrorCodeToStatusCode(string errorCode)
    {
        return errorCode switch
        {
            "VALIDATION_ERROR" => StatusCodes.Status400BadRequest,      // 400
            "NOT_FOUND" => StatusCodes.Status404NotFound,               // 404
            "DUPLICATE" => StatusCodes.Status409Conflict,               // 409
            "UNAUTHORIZED" => StatusCodes.Status401Unauthorized,        // 401
            "FORBIDDEN" => StatusCodes.Status403Forbidden,              // 403
            "DATABASE_ERROR" => StatusCodes.Status500InternalServerError, // 500
            _ => StatusCodes.Status500InternalServerError               // 500 por defecto
        };
    }
}