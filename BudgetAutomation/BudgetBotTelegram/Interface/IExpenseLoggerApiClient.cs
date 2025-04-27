using SharedLibrary;

namespace BudgetBotTelegram.Interface;

public interface IExpenseLoggerApiClient
{
    Task<Expense> LogExpenseAsync(Expense expense, CancellationToken cancellationToken = default);
}