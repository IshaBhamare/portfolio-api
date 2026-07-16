using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Portfolio_EmailService.Models;

namespace Portfolio_EmailService.Services;

public class EmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> emailOptions,
        HttpClient httpClient,
        ILogger<EmailService> logger)
    {
        _emailSettings = emailOptions.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendEmailAsync(EmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(_emailSettings.ApiKey) ||
            string.IsNullOrWhiteSpace(_emailSettings.FromEmail) ||
            string.IsNullOrWhiteSpace(_emailSettings.ToEmail))
        {
            throw new InvalidOperationException(
                "Email settings are missing. Configure EmailSettings:ApiKey, EmailSettings:FromEmail, and EmailSettings:ToEmail.");
        }

        var payload = new
        {
            from = _emailSettings.FromEmail,
            to = new[] { _emailSettings.ToEmail },
            reply_to = request.Email,
            subject = $"Portfolio Contact: {request.Subject}",
            html = BuildHtmlBody(request),
            text = BuildTextBody(request)
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "emails");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _emailSettings.ApiKey);
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        _logger.LogInformation("Sending email through Resend for contact request from {Email}", request.Email);

        using var response = await _httpClient.SendAsync(httpRequest);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Resend request failed with status {StatusCode}. Response: {Response}",
                (int)response.StatusCode,
                responseContent);

            throw new HttpRequestException(
                $"Resend API request failed with status {(int)response.StatusCode}: {responseContent}",
                null,
                response.StatusCode);
        }
    }

    private static string BuildTextBody(EmailRequest request)
    {
        return
            $"Name: {request.Name}\n" +
            $"Email: {request.Email}\n" +
            $"Subject: {request.Subject}\n\n" +
            $"Message:\n{request.Message}";
    }

    private static string BuildHtmlBody(EmailRequest request)
    {
        return
            "<h2>New Portfolio Contact Request</h2>" +
            $"<p><strong>Name:</strong> {EscapeHtml(request.Name)}</p>" +
            $"<p><strong>Email:</strong> {EscapeHtml(request.Email)}</p>" +
            $"<p><strong>Subject:</strong> {EscapeHtml(request.Subject)}</p>" +
            $"<p><strong>Message:</strong></p><p>{EscapeHtml(request.Message).Replace("\n", "<br />")}</p>";
    }

    private static string EscapeHtml(string value)
    {
        return System.Net.WebUtility.HtmlEncode(value);
    }
}
