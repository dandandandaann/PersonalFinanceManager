using SharedLibrary.Model;

namespace BudgetBotTelegram.Interface;

public interface IExpenseLoggerApiClient
{
    Task<Expense> LogExpenseAsync(string spreadsheetId, Expense expense, CancellationToken cancellationToken = default);
}