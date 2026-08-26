namespace Module.Auth.Application.Abstraction;

public class AuthenticationSettings
{
    public const string SectionName = "Authentication";
}

public class Auth0Settings
{
    public const string SectionName = "Auth0";
    public string Domain { get; set; } = string.Empty;
    public string Issuer {get;set;} = string.Empty ;
    public string Audience { get; set; } = string.Empty;
    public string SpaClientId { get; set; } = string.Empty;
    public string SpaClientSecret { get; set; } = string.Empty;
    public string Connection { get; set; } = "Username-Password-Authentication";
    public Auth0M2MSettings M2M { get; set; } = new();
}

public class Auth0M2MSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string? StaticAccessToken { get; set; }
}
