using ExpenseTracker.Models;
using ExpenseTracker.Services;
using MongoDB.Bson;

namespace ExpenseTracker.Endpoints;

/// <summary>
/// Route definitions for the expenses API. Kept out of Program.cs so the composition
/// root stays about wiring, not about individual routes.
/// </summary>
public static class ExpenseEndpoints
{
    public static IEndpointRouteBuilder MapExpenseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/expenses").WithTags("Expenses");

        // Collection
        group.MapGet("/", async (ExpenseService service) =>
        {
            var expenses = await service.GetAllAsync();
            return Results.Ok(expenses);
        })
        .WithName("GetAllExpenses")
        .WithOpenApi();

        group.MapPost("/", async (Expense expense, ExpenseService service) =>
        {
            var created = await service.CreateAsync(expense);
            return Results.Created($"/expenses/{created.Id}", created);
        })
        .WithName("CreateExpense")
        .WithOpenApi();

        // Literal routes are declared before /{id} for readability. Route matching ranks
        // literal segments above parameters, so order does not change behaviour.
        group.MapGet("/filter", async (DateTime start, DateTime end, ExpenseService service) =>
        {
            var expenses = await service.FilterByDateRangeAsync(start, end);
            return Results.Ok(expenses);
        })
        .WithName("FilterExpensesByDateRange")
        .WithOpenApi();

        group.MapGet("/summary", async (ExpenseService service) =>
        {
            var summary = await service.GetCategorySummaryAsync();
            return Results.Ok(summary);
        })
        .WithName("GetCategorySummary")
        .WithOpenApi();

        group.MapGet("/summary/filter", async (DateTime start, DateTime end, ExpenseService service) =>
        {
            var summary = await service.GetCategorySummaryByDateRangeAsync(start, end);
            return Results.Ok(summary);
        })
        .WithName("GetCategorySummaryByDateRange")
        .WithOpenApi();

        // Single item
        group.MapGet("/{id}", async (string id, ExpenseService service) =>
        {
            if (!IsValidId(id)) return InvalidId();

            var expense = await service.GetByIdAsync(id);
            return expense is not null ? Results.Ok(expense) : Results.NotFound();
        })
        .WithName("GetExpenseById")
        .WithOpenApi();

        group.MapPut("/{id}", async (string id, Expense expense, ExpenseService service) =>
        {
            if (!IsValidId(id)) return InvalidId();
            if (await service.GetByIdAsync(id) is null) return Results.NotFound();

            var updated = await service.UpdateAsync(id, expense);
            return updated ? Results.NoContent() : Results.BadRequest();
        })
        .WithName("UpdateExpense")
        .WithOpenApi();

        group.MapDelete("/{id}", async (string id, ExpenseService service) =>
        {
            if (!IsValidId(id)) return InvalidId();
            if (await service.GetByIdAsync(id) is null) return Results.NotFound();

            var deleted = await service.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.BadRequest();
        })
        .WithName("DeleteExpense")
        .WithOpenApi();

        return app;
    }

    // Mongo ids are ObjectIds. Without this check a malformed id reaches the driver, which
    // throws while converting it, and the client sees a 500 instead of a 400.
    private static bool IsValidId(string id) => ObjectId.TryParse(id, out _);

    private static IResult InvalidId() =>
        Results.BadRequest(new { error = "Invalid expense id." });
}
