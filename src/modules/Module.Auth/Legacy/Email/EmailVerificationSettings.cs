namespace Module.Auth.Application.Abstraction;

public class EmailVerificationSettings
{
    public bool Required { get; set; }
    public int TokenExpirationHours { get; set; } = 24;
    public int VerificationCodeLength { get; set; } = 6;
    public List<string> RequiredForProviders { get; set; } = new() { "Local" };
}
