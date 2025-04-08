using BudgetBotTelegram.Model;

namespace BudgetBotTelegram.Interface;

public interface IExpenseLoggerApiClient
{
    Task LogExpenseAsync(Expense expense, CancellationToken cancellationToken = default);
}