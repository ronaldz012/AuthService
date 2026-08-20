namespace Common.Contracts.authentication;

/// <summary>
/// Contexto del usuario autenticado resuelto por el TenantMiddleware desde
/// el token externo (Auth0) + la sesión. Es la fuente de verdad para ICurrentUser.
/// </summary>
public record CurrentUserContext(
    Guid TenantId,
    Guid UserId,
    string FullName,
    string Username,
    int UserType,
    string? ExternalAuthId);

public static class CurrentUserContextKeys
{
    public const string HttpContextKey = "CurrentUserContext";
}
