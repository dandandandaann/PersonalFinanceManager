using ExpenseLoggerApi.Interface;
using SharedLibrary.Constants;
using SharedLibrary.Dto;
using SharedLibrary.Enum;
using SharedLibrary.Model;

namespace ExpenseLoggerApi.Service;

public class SpreadsheetService(ISheetsDataAccessor sheetsAccessor, ILogger<SpreadsheetService> logger)
{
    public async Task<SpreadsheetValidationResponse> ValidateSpreadsheetId(string spreadsheetId)
    {
        var request = new SpreadsheetValidationRequest { SpreadsheetId = spreadsheetId };
        var response = await sheetsAccessor.ValidateSpreadsheetIdAsync(request);

        return response;
    }
    public async Task<RemoveExpenseResponse> RemoveLastExpense(string spreadsheetId)
    {
        var transactionsSheet = SpreadsheetConstants.Sheets.Transactions;
        logger.LogInformation("Removing expense process in spreadsheet '{SpreadsheetId}'.", spreadsheetId);

        try
        {
            var sheetId = await sheetsAccessor.GetSheetIdByNameAsync(spreadsheetId, transactionsSheet);

            var lastRow = await sheetsAccessor.FindLastItemAsync(
                spreadsheetId, transactionsSheet, SpreadsheetConstants.Column.Description, SpreadsheetConstants.DataStartRow);

            if (lastRow < SpreadsheetConstants.DataStartRow)
            {
                logger.LogWarning("No expenses found to remove in spreadsheet '{SpreadsheetId}'.", transactionsSheet);
                throw new InvalidOperationException("No expense found.");
            }

            var values = await sheetsAccessor.ReadRowValuesAsync(spreadsheetId, transactionsSheet, lastRow);

            if (values == null || values.Count == 0)
            {
                logger.LogWarning("Empty or invalid row");
                throw new InvalidOperationException("Could not retrieve expense data.");
            }

            // Apaga a linha só depois de garantir os dados
            await sheetsAccessor.DeleteRowAsync(spreadsheetId, sheetId, lastRow);

            var expense = new Expense
            {
                Description = values.ElementAtOrDefault((int)ExpenseColumnIndex.Description)?.ToString().Trim(),
                Amount = values.ElementAtOrDefault((int)ExpenseColumnIndex.Amount)?.ToString().Trim(),
                Category = values.ElementAtOrDefault((int)ExpenseColumnIndex.Category)?.ToString().Trim()
            };

            return new RemoveExpenseResponse
            {
                Success = true,
                expense = expense
            };

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove last logged expense.");
            throw;
        }
    }
}