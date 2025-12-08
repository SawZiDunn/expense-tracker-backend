using ExpenseTracker.Models;
using ExpenseTracker.Services;
using ExpenseTracker.Settings;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file (for local development)
if (File.Exists(".env"))
{
    DotNetEnv.Env.Load();
}

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
    options.ConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") 
                             ?? config["ConnectionString"] 
                             ?? throw new InvalidOperationException("MongoDB connection string not configured");
    options.DatabaseName = config["DatabaseName"] ?? "ExpenseTrackerDB";
    options.ExpensesCollection = config["ExpensesCollection"] ?? "expenses";
});

// Register ExpenseService
builder.Services.AddSingleton<ExpenseService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        var allowedOrigins = new List<string> 
        { 
            "http://localhost:5173",
            "http://localhost:5174",
            "https://expense-tracker-sawzidunn.vercel.app"
        };
        
        // Add FRONTEND_URL if specified
        var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
        System.Console.WriteLine($"Adding FRONTEND_URL to CORS allowed origins: {frontendUrl}");
        if (!string.IsNullOrEmpty(frontendUrl) && !allowedOrigins.Contains(frontendUrl))
        {
            
            allowedOrigins.Add(frontendUrl);
        }
        
        policy.WithOrigins(allowedOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure JSON serialization with camelCase
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

// Enable CORS
app.UseCors("AllowReactApp");

// Endpoints

// GET /expenses - Get all expenses
app.MapGet("/expenses", async (ExpenseService service) =>
{
    var expenses = await service.GetAllAsync();
    return Results.Ok(expenses);
})
.WithName("GetAllExpenses")
.WithOpenApi();

// GET /expenses/{id} - Get expense by ID
app.MapGet("/expenses/{id}", async (string id, ExpenseService service) =>
{
    var expense = await service.GetByIdAsync(id);
    return expense is not null ? Results.Ok(expense) : Results.NotFound();
})
.WithName("GetExpenseById")
.WithOpenApi();

// POST /expenses - Create new expense
app.MapPost("/expenses", async (Expense expense, ExpenseService service) =>
{
    var created = await service.CreateAsync(expense);
    return Results.Created($"/expenses/{created.Id}", created);
})
.WithName("CreateExpense")
.WithOpenApi();

// PUT /expenses/{id} - Update expense
app.MapPut("/expenses/{id}", async (string id, Expense expense, ExpenseService service) =>
{
    var exists = await service.GetByIdAsync(id);
    if (exists is null)
        return Results.NotFound();

    var updated = await service.UpdateAsync(id, expense);
    return updated ? Results.NoContent() : Results.BadRequest();
})
.WithName("UpdateExpense")
.WithOpenApi();

// DELETE /expenses/{id} - Delete expense
app.MapDelete("/expenses/{id}", async (string id, ExpenseService service) =>
{
    var exists = await service.GetByIdAsync(id);
    if (exists is null)
        return Results.NotFound();

    var deleted = await service.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.BadRequest();
})
.WithName("DeleteExpense")
.WithOpenApi();

// GET /expenses/filter?start=2024-01-01&end=2024-01-31 - Filter by date range
app.MapGet("/expenses/filter", async (DateTime start, DateTime end, ExpenseService service) =>
{
    var expenses = await service.FilterByDateRangeAsync(start, end);
    return Results.Ok(expenses);
})
.WithName("FilterExpensesByDateRange")
.WithOpenApi();

// GET /expenses/summary - Get category summary
app.MapGet("/expenses/summary", async (ExpenseService service) =>
{
    var summary = await service.GetCategorySummaryAsync();
    return Results.Ok(summary);
})
.WithName("GetCategorySummary")
.WithOpenApi();

// GET /expenses/summary/filter?start=2024-01-01&end=2024-01-31 - Get category summary by date range
app.MapGet("/expenses/summary/filter", async (DateTime start, DateTime end, ExpenseService service) =>
{
    var summary = await service.GetCategorySummaryByDateRangeAsync(start, end);
    return Results.Ok(summary);
})
.WithName("GetCategorySummaryByDateRange")
.WithOpenApi();

app.Run();
