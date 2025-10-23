


namespace Auth.Infrastructure.Authentication;

    public class TokenSettings

    {
        public const string SectionName = "Token";
        public string Key { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public int ExpirationMinutes { get; set; } = 1440;
    }
