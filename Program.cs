using Portfolio_EmailService.Models;
using Portfolio_EmailService.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services
    .AddOptions<EmailSettings>()
    .Bind(builder.Configuration.GetSection("EmailSettings"))
    .ValidateDataAnnotations()
    .Validate(
        settings => !string.IsNullOrWhiteSpace(settings.ApiKey) &&
                    !string.IsNullOrWhiteSpace(settings.FromEmail) &&
                    !string.IsNullOrWhiteSpace(settings.ToEmail),
        "Email settings must be configured.")
    .ValidateOnStart();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddHttpClient<EmailService>(client =>
{
    client.BaseAddress = new Uri("https://api.resend.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.UseSwagger();
app.UseSwaggerUI();

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapPost("/api/email/send", async (EmailRequest request, EmailService emailService) =>
{
    Log.Information("Email request received from {Email}", request.Email);

    if (string.IsNullOrWhiteSpace(request.Name) ||
        string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.Subject) ||
        string.IsNullOrWhiteSpace(request.Message))
    {
        Log.Warning("Validation failed for email request");
        return Results.BadRequest(new { message = "All fields are required" });
    }

    try
    {
        await emailService.SendEmailAsync(request);
        Log.Information("Email sent successfully for {Email}", request.Email);

        return Results.Ok(new { message = "Email sent successfully!" });
    }
    catch (InvalidOperationException ex)
    {
        Log.Error(ex, "Email configuration is invalid");
        return Results.Problem(
            detail: app.Environment.IsDevelopment() ? ex.Message : "Email service is not configured correctly.",
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Email configuration error");
    }
    catch (HttpRequestException ex)
    {
        Log.Error(ex, "Email provider request failed");
        return Results.Problem(
            detail: app.Environment.IsDevelopment() ? ex.Message : "Email provider request failed.",
            statusCode: StatusCodes.Status502BadGateway,
            title: "Unable to send email");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error while sending email");
        return Results.Problem(
            detail: app.Environment.IsDevelopment() ? ex.Message : "Unexpected server error while processing email.",
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Unexpected error");
    }
});

app.Run();
