using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using PitStop.Application.Interfaces;

namespace PitStop.Infrastructure.Services;

public class SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger) : IEmailService
{
    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var section = config.GetSection("Email");
        var host = section["Host"] ?? "localhost";
        var port = int.Parse(section["Port"] ?? "587");
        var user = section["Username"] ?? string.Empty;
        var pass = section["Password"] ?? string.Empty;
        var from = section["From"] ?? "noreply@pitstop.ro";
        var fromName = section["FromName"] ?? "PitStop";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, from));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            if (!string.IsNullOrEmpty(user))
                await client.AuthenticateAsync(user, pass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            logger.LogInformation("Email sent to {To} subject={Subject}", to, subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To} subject={Subject}", to, subject);
            throw;
        }
    }
}