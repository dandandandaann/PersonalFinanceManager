using SharedLibrary.Model;

namespace BudgetAutomation.Engine.Interface;

public interface IExpenseLoggerApiClient
{
    Task<Expense> LogExpenseAsync(string spreadsheetId, Expense expense, CancellationToken cancellationToken = default);
}