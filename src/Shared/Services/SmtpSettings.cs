using System;

namespace Shared.Services;

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
//   "SmtpSettings": {
//   "Host": "smtp.tudominio.com",
//   "Port": 587,
//   "Username": "tu_correo_smtp@dominio.com", 
//   "Password": "tu_contraseña_smtp",       
//   "EnableSsl": true,                       
//   "FromAddress": "No-Responder <no-responder@tudomiPnio.com>"
//   },