using Portfolio_EmailService.Models;
using Portfolio_EmailService.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ✅ Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ✅ Add services BEFORE build
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ CORS
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

// ✅ Register Email Service
builder.Services.AddScoped<EmailService>();

var app = builder.Build();

// ✅ Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// ✅ API endpoint
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
        Log.Information("Email sent successfully to {Email}", request.Email);

        return Results.Ok(new { message = "Email sent successfully!" });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error while sending email");
        return Results.Problem("Something went wrong");
    }
});

app.Run();