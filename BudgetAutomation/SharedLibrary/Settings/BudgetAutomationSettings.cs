namespace SharedLibrary.Settings;

public class BudgetAutomationSettings
{
    public const string Configuration = "BudgetAutomation";

    public BotSettings botSettings { get; set; } = null!;
    public ExpenseLoggerApiClientSettings ExpenseLoggerApiClientSettings { get; set; } = null!;
    public ExpenseLoggerSettings expenseLoggerSettings { get; set; } = null!;
    public UserApiClientSettings userApiClientSettings { get; set; } = null!;
}