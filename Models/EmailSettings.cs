using System.ComponentModel.DataAnnotations;

namespace Portfolio_EmailService.Models;

public class EmailSettings
{
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string FromEmail { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string ToEmail { get; set; } = string.Empty;
}
