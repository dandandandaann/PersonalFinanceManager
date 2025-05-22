using SharedLibrary.Dto;
using SharedLibrary.Model;

namespace BudgetAutomation.Engine.Interface;


public interface IExpenseLoggerApiClient
{
    Task<LogExpenseResponse> LogExpenseAsync(string spreadsheetId, Expense expense, CancellationToken cancellationToken = default);
}