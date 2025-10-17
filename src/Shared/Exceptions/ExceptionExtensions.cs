using System;

namespace Shared.Extensions;

using Microsoft.EntityFrameworkCore;
using Shared.Result;

/// <summary>
/// Extensiones para convertir excepciones en objetos Error del patrón ROP.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Convierte cualquier excepción en un Error apropiado según su tipo.
    /// </summary>
    public static Error ToError(this Exception ex)
    {
        return ex switch
        {
            DbUpdateException dbEx => dbEx.ToDbError(),
            InvalidOperationException => new Error("INVALID_OPERATION", ex.GetInnerMostMessage()),
            UnauthorizedAccessException => new Error("UNAUTHORIZED", "No tiene permisos para realizar esta operación"),
            ArgumentNullException argEx => new Error("VALIDATION_ERROR", $"El parámetro '{argEx.ParamName}' es requerido"),
            ArgumentException argEx => new Error("VALIDATION_ERROR", argEx.Message),
            KeyNotFoundException => new Error("NOT_FOUND", ex.Message),
            TimeoutException => new Error("TIMEOUT", "La operación tardó demasiado tiempo"),
            OperationCanceledException => new Error("CANCELLED", "La operación fue cancelada"),
            _ => new Error("UNEXPECTED_ERROR", $"Error inesperado: {ex.GetInnerMostMessage()}")
        };
    }

    /// <summary>
    /// Convierte una DbUpdateException en un Error específico de base de datos.
    /// Detecta violaciones de constraints y da mensajes apropiados.
    /// </summary>
    public static Error ToDbError(this DbUpdateException ex)
    {
        var innerMessage = ex.GetInnerMostMessage();
        
        // Detectar violación de Foreign Key
        if (IsConstraintViolation(innerMessage, "FOREIGN KEY", "FK_"))
        {
            return new Error("INVALID_REFERENCE", 
                "La referencia especificada no existe o es inválida");
        }
        
        // Detectar violación de Unique Constraint
        if (IsConstraintViolation(innerMessage, "UNIQUE", "duplicate", "IX_", "UQ_"))
        {
            return new Error("DUPLICATE", 
                "Ya existe un registro con estos datos únicos");
        }
        
        // Detectar violación de Check Constraint
        if (IsConstraintViolation(innerMessage, "CHECK", "CK_"))
        {
            return new Error("INVALID_DATA", 
                "Los datos no cumplen con las reglas de validación");
        }

        // Detectar violación de Primary Key
        if (IsConstraintViolation(innerMessage, "PRIMARY KEY", "PK_"))
        {
            return new Error("DUPLICATE", 
                "Ya existe un registro con ese identificador");
        }

        // Detectar timeout de base de datos
        if (innerMessage.Contains("timeout", StringComparison.OrdinalIgnoreCase))
        {
            return new Error("DATABASE_TIMEOUT", 
                "La operación en la base de datos tardó demasiado");
        }

        // Detectar problemas de conexión
        if (innerMessage.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
            innerMessage.Contains("network", StringComparison.OrdinalIgnoreCase))
        {
            return new Error("DATABASE_CONNECTION", 
                "No se pudo conectar a la base de datos");
        }

        // Error genérico de base de datos
        return new Error("DATABASE_ERROR", 
            $"Error al guardar en la base de datos: {innerMessage}");
    }

    /// <summary>
    /// Obtiene el mensaje de la excepción más interna (donde está el error real).
    /// </summary>
    public static string GetInnerMostMessage(this Exception ex)
    {
        var innerException = ex;
        
        // Navegar hasta la excepción más interna
        while (innerException.InnerException != null)
        {
            innerException = innerException.InnerException;
        }
        
        return innerException.Message;
    }

    /// <summary>
    /// Verifica si el mensaje contiene alguna de las palabras clave de violación de constraint.
    /// </summary>
    private static bool IsConstraintViolation(string message, params string[] keywords)
    {
        return keywords.Any(keyword => 
            message.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}