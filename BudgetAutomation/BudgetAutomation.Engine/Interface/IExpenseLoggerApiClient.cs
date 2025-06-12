using SharedLibrary.Dto;
using SharedLibrary.Model;

namespace BudgetAutomation.Engine.Interface;


public interface IExpenseLoggerApiClient
{
    Task<LogExpenseResponse> LogExpenseAsync(string spreadsheetId, Expense expense, CancellationToken cancellationToken = default);
    Task<SpreadsheetValidationResponse> ValidateSpreadsheet(string spreadsheetId, CancellationToken cancellationToken = default);
    Task<RemoveExpenseResponse> RemoveLastExpenseAsync(string spreadsheetId, CancellationToken cancellationToken = default);
}