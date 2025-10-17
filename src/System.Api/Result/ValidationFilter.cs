using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace System.Api.Result;


/// <summary>
/// Filtro que intercepta errores de validación de ModelState (Data Annotations)
/// y los convierte al formato ProblemDetails estándar.
/// </summary>
public class ValidationFilter : IActionFilter
{
    /// <summary>
    /// Se ejecuta ANTES de que la acción del controller se ejecute.
    /// Aquí interceptamos si ModelState tiene errores de validación.
    /// </summary>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Si el ModelState es inválido (Data Annotations fallaron)
        if (!context.ModelState.IsValid)
        {
            // Extraemos todos los mensajes de error
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();

            // Creamos un ProblemDetails con el mismo formato que tus errores
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "VALIDATION_ERROR",
                Detail = string.Join("; ", errors) // Combinamos todos los errores
            };

            // Cortamos la ejecución y retornamos 400 BadRequest
            context.Result = new BadRequestObjectResult(problemDetails);
        }
    }

    /// <summary>
    /// Se ejecuta DESPUÉS de la acción. No lo necesitamos para validación.
    /// </summary>
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No hacemos nada aquí
    }
}