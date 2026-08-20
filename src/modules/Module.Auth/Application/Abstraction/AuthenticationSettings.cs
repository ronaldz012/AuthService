namespace Module.Auth.Application.Abstraction;

public class AuthenticationSettings
{
    public const string SectionName = "Authentication";
    public EmailVerificationSettings EmailVerification { get; set; } = new();
    public Google Google { get; set; } = new();
}

public class EmailVerificationSettings
{
    public bool Required { get; set; }
    public int TokenExpirationHours { get; set; } = 24;
    public int VerificationCodeLength { get; set; } = 6;
    public List<string> RequiredForProviders { get; set; } = new() { "Local" };
}

public class Google
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public bool TrustEmailVerification { get; set; } = true;
}

public class Auth0Settings
{
    public const string SectionName = "Auth0";
    public string Domain { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SpaClientSecret { get; set; } = string.Empty;
}
