using System;
using System.Net;
using System.Net.Mail;
using Portfolio_EmailService.Models;

namespace Portfolio_EmailService.Services;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(EmailRequest request)
    {
        var senderEmail = _config["EmailSettings:SenderEmail"];
        var senderPassword = _config["EmailSettings:AppPassword"];

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail),
            Subject = $"Portfolio Contact: {request.Subject}",
            Body = $"Name: {request.Name}\nEmail: {request.Email}\n\nMessage:\n{request.Message}",
            IsBodyHtml = false,
        };

        // Send to yourself
        mailMessage.To.Add(senderEmail);

        using var smtpClient = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = true
        };

        await smtpClient.SendMailAsync(mailMessage);
    }
}
