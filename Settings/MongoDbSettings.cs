namespace ExpenseTracker.Settings;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string ExpensesCollection { get; set; } = string.Empty;
}
