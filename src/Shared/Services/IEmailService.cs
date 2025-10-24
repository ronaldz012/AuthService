using System;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Shared.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
}

public class EmailService(IOptions<SmtpSettings> smtpSettings) : IEmailService
{
    private readonly SmtpSettings _smtpSettings = smtpSettings.Value;

    public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
    {
        if (string.IsNullOrWhiteSpace(toEmail) || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("ToEmail, Subject, and Body cannot be null or empty for sending an email.");

        if (string.IsNullOrWhiteSpace(_smtpSettings.FromAddress))
            throw new InvalidOperationException("FromAddress must be configured in SMTP settings");

        // Crear el mensaje usando MimeKit
        var message = new MimeMessage();
            message.From.Add(new MailboxAddress("", _smtpSettings.FromAddress));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;
            message.Body = new TextPart(isHtml ? "html" : "plain")
            {
                Text = body
            };
        

        using (var smtpClient = new SmtpClient())
        {
            try
            {
                smtpClient.Timeout = 10000; 

                var secureSocketOptions = _smtpSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
                
                if (_smtpSettings.Port == 465)
                {
                    secureSocketOptions = SecureSocketOptions.SslOnConnect;
                }
                await smtpClient.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, secureSocketOptions);
                await smtpClient.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);

                await smtpClient.SendAsync(message);
            }
            catch (MailKit.Security.AuthenticationException authEx)
            {
                throw new InvalidOperationException("SMTP authentication failed. Please check your username and password.", authEx);
            }
            catch (MailKit.Net.Smtp.SmtpCommandException smtpEx)
            {
                throw new InvalidOperationException($"SMTP command failed: {smtpEx.StatusCode} - {smtpEx.Message}", smtpEx);
            }
            catch (MailKit.Net.Smtp.SmtpProtocolException protoEx)
            {
                throw new InvalidOperationException($"SMTP protocol error: {protoEx.Message}", protoEx);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
            }
            finally
            {
                if (smtpClient.IsConnected)
                        await smtpClient.DisconnectAsync(true);
            }
        }
    }
}
