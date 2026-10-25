using ExpenseTracker.Endpoints;
using ExpenseTracker.Services;
using ExpenseTracker.Settings;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;

// Load .env first: the host reads ASPNETCORE_ENVIRONMENT and other settings as soon as
// it is created, so anything set after CreateBuilder would arrive too late.
if (File.Exists(".env"))
{
    DotNetEnv.Env.Load();
}

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use PORT environment variable (for Render deployment)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(int.Parse(port));
});

// MongoDB settings - use environment variable if available, otherwise use appsettings.json
builder.Services.Configure<MongoDbSettings>(options =>
{
    var config = builder.Configuration.GetSection("MongoDbSettings");

    // Fail loudly when no connection string is configured.
    var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        connectionString = config["ConnectionString"];
    }

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "MongoDB connection string is not configured. Set MONGODB_CONNECTION_STRING " +
            "or MongoDbSettings:ConnectionString in appsettings.json.");
    }

    options.ConnectionString = connectionString;
    options.DatabaseName = config["DatabaseName"] ?? "ExpenseTrackerDB";
    options.ExpensesCollection = config["ExpensesCollection"] ?? "expenses";
});

// Register ExpenseService
builder.Services.AddSingleton<ExpenseService>();

// CORS - origins come from configuration so a new deployment never needs a code change.
// CORS_ORIGINS is a comma-separated list; FRONTEND_URL is still honoured so the existing
// Compose file and render.yaml keep working without edits.
var allowedOrigins = (Environment.GetEnvironmentVariable("CORS_ORIGINS")
                   ?? Environment.GetEnvironmentVariable("FRONTEND_URL")
                   ?? string.Empty)
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Select(origin => origin.TrimEnd('/')) // a trailing slash silently fails origin matching
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

// Convenience for local development only - never active outside Development.
if (allowedOrigins.Length == 0 && builder.Environment.IsDevelopment())
{
    allowedOrigins = ["http://localhost:5173", "http://localhost:5174"];
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

// Configure JSON serialization with camelCase
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

app.Logger.LogInformation(
    "CORS allowed origins: {Origins}",
    allowedOrigins.Length == 0 ? "(none)" : string.Join(", ", allowedOrigins));

// Resolve the options once at startup so a missing connection string fails the boot
// instead of surfacing on the first request.
_ = app.Services.GetRequiredService<IOptions<MongoDbSettings>>().Value;

// Global error handling.
// Without this, an unhandled exception produces a bare 500 with no body and no CORS
// headers, which the browser reports as a misleading "CORS policy" error. Returning a
// JSON body keeps the real failure visible.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        logger.LogError(feature?.Error,
            "Unhandled exception while handling {Method} {Path}",
            context.Request.Method,
            context.Request.Path);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = "Internal server error" });
    });
});

// Enable CORS
app.UseCors("AllowReactApp");

// Endpoints
app.MapExpenseEndpoints();

app.Run();
