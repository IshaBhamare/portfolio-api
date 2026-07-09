using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Portfolio_EmailService.Models;

namespace Portfolio_EmailService.Services;

public class EmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailOptions, ILogger<EmailService> logger)
    {
        _emailSettings = emailOptions.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(EmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(_emailSettings.SenderEmail) ||
            string.IsNullOrWhiteSpace(_emailSettings.AppPassword))
        {
            throw new InvalidOperationException(
                "Email settings are missing. Configure EmailSettings:SenderEmail and EmailSettings:AppPassword.");
        }

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_emailSettings.SenderEmail),
            Subject = $"Portfolio Contact: {request.Subject}",
            Body = $"Name: {request.Name}\nEmail: {request.Email}\n\nMessage:\n{request.Message}",
            IsBodyHtml = false
        };

        mailMessage.To.Add(_emailSettings.SenderEmail);

        using var smtpClient = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.AppPassword),
            EnableSsl = true
        };

        _logger.LogInformation("Attempting SMTP connection to Gmail for sender {SenderEmail}", _emailSettings.SenderEmail);
        await smtpClient.SendMailAsync(mailMessage);
    }
}
