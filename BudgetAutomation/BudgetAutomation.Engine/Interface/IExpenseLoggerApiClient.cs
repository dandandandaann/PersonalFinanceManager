using SharedLibrary.Dto;
using SharedLibrary.Model;

namespace BudgetAutomation.Engine.Interface;


public interface ISpreadsheetManagerApiClient
{
    Task<LogExpenseResponse> LogExpenseAsync(string spreadsheetId, Expense expense, CancellationToken cancellationToken = default);
    Task<SpreadsheetValidationResponse> ValidateSpreadsheet(string spreadsheetId, CancellationToken cancellationToken = default);
    Task<RemoveExpenseResponse> RemoveLastExpenseAsync(string spreadsheetId, CancellationToken cancellationToken = default);
    Task<ExpenseResponse> GetLastExpenseAsync(string spreadsheetId, CancellationToken cancellationToken = default);
    Task<AddCategoryRuleResponse> AddCategoryRuleAsync(string spreadsheetId, string category, string descriptionPattern, CancellationToken cancellationToken = default);
}