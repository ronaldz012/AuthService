using System;
using System.Threading.Tasks;
using Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace System.Api.Result;

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
            Title = result.Error.Code.ToString(), // Convierte el Enum a String (ej: "ValidationError")
            Detail = result.Error.Message         // Mensaje descriptivo del UseCase
        })
        {
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Mapea los códigos del enum ErrorCode a códigos de estado HTTP de forma tipada.
    /// </summary>
    /// <param name="errorCode">Código genérico del error</param>
    /// <returns>Código de estado HTTP apropiado</returns>
    private static int MapErrorCodeToStatusCode(ErrorCode errorCode)
    {
        return errorCode switch
        {
            ErrorCode.ValidationError => StatusCodes.Status400BadRequest,
            ErrorCode.BadRequest => StatusCodes.Status400BadRequest,
            ErrorCode.InvalidState => StatusCodes.Status400BadRequest,
            
            ErrorCode.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorCode.Forbidden => StatusCodes.Status403Forbidden,
            ErrorCode.NotFound => StatusCodes.Status404NotFound,
            
            ErrorCode.Duplicate => StatusCodes.Status409Conflict,
            ErrorCode.Conflict => StatusCodes.Status409Conflict,
            
            ErrorCode.DatabaseError => StatusCodes.Status500InternalServerError,
            ErrorCode.InternalError => StatusCodes.Status500InternalServerError,
            
            _ => StatusCodes.Status500InternalServerError
        };
    }
}