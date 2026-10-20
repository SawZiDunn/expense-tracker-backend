using ExpenseTracker.Models;
using ExpenseTracker.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ExpenseTracker.Services;

public class ExpenseService
{
    private readonly IMongoCollection<Expense> _expensesCollection;

    public ExpenseService(IOptions<MongoDbSettings> mongoDbSettings)
    {
        var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
        _expensesCollection = mongoDatabase.GetCollection<Expense>(mongoDbSettings.Value.ExpensesCollection);
    }

    public async Task<List<Expense>> GetAllAsync() =>
        await _expensesCollection.Find(_ => true).ToListAsync();

    public async Task<Expense?> GetByIdAsync(string id) =>
        await _expensesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<Expense> CreateAsync(Expense expense)
    {
        await _expensesCollection.InsertOneAsync(expense);
        return expense;
    }

    public async Task<bool> UpdateAsync(string id, Expense expense)
    {
        expense.Id = id;
        var result = await _expensesCollection.ReplaceOneAsync(x => x.Id == id, expense);

        // MatchedCount, not ModifiedCount: replacing a document with identical values
        // leaves ModifiedCount at 0, which would report a valid update as a failure.
        return result.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _expensesCollection.DeleteOneAsync(x => x.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<List<Expense>> FilterByDateRangeAsync(DateTime start, DateTime end)
    {
        var filter = Builders<Expense>.Filter.And(
            Builders<Expense>.Filter.Gte(x => x.Date, start),
            Builders<Expense>.Filter.Lte(x => x.Date, end)
        );
        return await _expensesCollection.Find(filter).ToListAsync();
    }

    public async Task<Dictionary<string, decimal>> GetCategorySummaryAsync()
    {
        var expenses = await GetAllAsync();
        return expenses
            .GroupBy(e => e.Category)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
    }

    public async Task<Dictionary<string, decimal>> GetCategorySummaryByDateRangeAsync(DateTime start, DateTime end)
    {
        var expenses = await FilterByDateRangeAsync(start, end);
        return expenses
            .GroupBy(e => e.Category)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
    }
}
