using System.ComponentModel.DataAnnotations;

namespace Portfolio_EmailService.Models;

public class EmailSettings
{
    [Required]
    [EmailAddress]
    public string SenderEmail { get; set; } = string.Empty;

    [Required]
    public string AppPassword { get; set; } = string.Empty;
}
