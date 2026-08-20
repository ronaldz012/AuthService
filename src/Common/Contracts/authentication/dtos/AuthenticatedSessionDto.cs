namespace Common.Contracts.authentication.dtos;

/// <summary>
/// Envuelve la sesión del usuario autenticado junto con los datos necesarios
/// para establecer la conexión al schema del tenant. Schema/DatabaseName NO se
/// exponen al frontend (quedan fuera de SessionStateDto).
/// </summary>
public class AuthenticatedSessionDto
{
    public SessionStateDto Session { get; set; } = null!;
    public string Schema { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string ExternalAuthId { get; set; } = string.Empty;
}
