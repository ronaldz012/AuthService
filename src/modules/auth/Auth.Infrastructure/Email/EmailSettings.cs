using System;

namespace Auth.Infrastructure.Email;

public class SmtpSettings
{
    public const string SectionName = "SmtpSettings";
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; }
    public string FromAddress { get; set; } = string.Empty;
}
