using System;

namespace Portfolio_EmailService.Models;

public class EmailRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
}
