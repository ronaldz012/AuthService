using System;

namespace Auth.Infrastructure.Authentication;


/// <summary>
/// Configuración de autenticación y verificación de email.
/// </summary>
public class AuthenticationSettings
{
    public const string SectionName = "Authentication";
    public EmailVerificationSettings EmailVerification { get; set; } = new();
    public OAuthSettings OAuth { get; set; } = new();
}

public class EmailVerificationSettings
{
    /// <summary>
    /// Indica si la verificación de email está habilitada.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// Horas de validez del token de verificación.
    /// </summary>
    public int TokenExpirationHours { get; set; } = 24;

    public int VerificationCodeLength { get; set; } = 6;
    
    /// <summary>
    /// Proveedores que requieren verificación de email.
    /// </summary>
    public List<string> RequiredForProviders { get; set; } = new() { "Local" };
}

public class OAuthSettings
{
    public OAuthProviderSettings Google { get; set; } = new();
    public OAuthProviderSettings Facebook { get; set; } = new();
}

public class OAuthProviderSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    
    /// <summary>
    /// Indica si confiamos en la verificación de email del proveedor externo.
    /// </summary>
    public bool TrustEmailVerification { get; set; } = true;
}