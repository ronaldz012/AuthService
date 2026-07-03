using System.Diagnostics.CodeAnalysis;

namespace Common.Utilities;

/// <summary>
/// Categorías genéricas de error para todo el sistema que mapean a comportamientos HTTP.
/// </summary>
public enum ErrorCode
{
    // --- Mapean a 400 Bad Request ---
    ValidationError,     // Errores de validación de inputs, reglas de negocio sencillas
    BadRequest,          // Peticiones mal formadas o lógicamente imposibles
    InvalidState,        // El sistema no está en el estado correcto para ejecutar la acción
    
    // --- Mapean a otros códigos 4xx ---
    Unauthorized,        // 401: Falta autenticación o credenciales inválidas
    Forbidden,           // 403: Autenticado pero no tiene permisos para el recurso
    NotFound,            // 404: El recurso, entidad o registro no existe
    Duplicate,           // 409: Conflicto por llaves duplicadas o datos únicos
    Conflict,            // 409: Conflicto de concurrencia o de negocio general

    // --- Mapean a 5xx Server Errors ---
    DatabaseError,       // 500: Fallos explícitos al interactuar con el motor de persistencia
    InternalError        // 500: Errores inesperados o excepciones no controladas
}

/// <summary>
/// Representa un error con un código identificador fuertemente tipado y un mensaje descriptivo.
/// </summary>
/// <param name="Code">Código único del error basado en un catálogo cerrado</param>
/// <param name="Message">Mensaje descriptivo del error especializado por cada UseCase</param>
public record Error(ErrorCode Code, string Message);

/// <summary>
/// Representa el resultado de una operación que puede tener éxito o fallar.
/// Implementa el patrón Railway Oriented Programming (ROP).
/// </summary>
/// <typeparam name="TValue">Tipo del valor cuando la operación es exitosa</typeparam>
public readonly record struct Result<TValue>
{
    /// <summary>
    /// Valor cuando la operación fue exitosa. Será null si falló.
    /// </summary>
    public TValue? Value { get; }

    /// <summary>
    /// Información del error cuando la operación falló. Será null si fue exitosa.
    /// </summary>
    public Error? Error { get; }

    /// <summary>
    /// Indica si la operación fue exitosa (true) o falló (false).
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    /// <summary>
    /// Constructor privado para crear un resultado exitoso.
    /// Solo se usa internamente a través de la conversión implícita.
    /// </summary>
    /// <param name="value">Valor de retorno exitoso</param>
    private Result(TValue value)
    {
        Value = value;
        IsSuccess = true;
        Error = null;
    }

    /// <summary>
    /// Constructor privado para crear un resultado fallido.
    /// Solo se usa internamente a través de la conversión implícita.
    /// </summary>
    /// <param name="error">Información del error</param>
    private Result(Error error)
    {
        Error = error;
        IsSuccess = false;
        Value = default;
    }

    /// <summary>
    /// Permite convertir automáticamente un valor al tipo Result exitoso.
    /// </summary>
    public static implicit operator Result<TValue>(TValue value) => new(value);

    /// <summary>
    /// Permite convertir automáticamente un Error al tipo Result fallido.
    /// </summary>
    public static implicit operator Result<TValue>(Error error) => new(error);
}