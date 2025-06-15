using SpreadsheetManagerApi.Interface;
using SpreadsheetManagerApi.Misc;
using SharedLibrary.Constants;
using SharedLibrary.Dto;
using SharedLibrary.Enum;
using SharedLibrary.Model;

namespace SpreadsheetManagerApi.Service;

public class SpreadsheetService(ISheetsDataAccessor sheetsAccessor, ILogger<SpreadsheetService> logger)
{
    public async Task<SpreadsheetValidationResponse> ValidateSpreadsheetId(string spreadsheetId)
    {
        var request = new SpreadsheetValidationRequest { SpreadsheetId = spreadsheetId };
        var response = await sheetsAccessor.ValidateSpreadsheetIdAsync(request);

        return response;
    }
    public async Task<RemoveExpenseResponse> RemoveLastExpenseAsync(string spreadsheetId)
    {
        var transactionsSheet = SpreadsheetConstants.Transactions.SheetName;
        logger.LogInformation("Removing expense process in spreadsheet '{SpreadsheetId}'.", spreadsheetId);

        try
        {
            var sheetId = await sheetsAccessor.GetSheetIdByNameAsync(spreadsheetId, transactionsSheet);

            var lastRow = await sheetsAccessor.FindLastItemAsync(
                spreadsheetId, transactionsSheet, SpreadsheetConstants.Transactions.Column.Description, SpreadsheetConstants.Transactions.DataStartRow);

            if (lastRow < SpreadsheetConstants.Transactions.DataStartRow)
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
                Description = values.ElementAtOrDefault(SpreadsheetConstants.Transactions.Column.Description.LetterToColumnIndex()).ToString().Trim(),
                Amount = values.ElementAtOrDefault(SpreadsheetConstants.Transactions.Column.Amount.LetterToColumnIndex()).ToString().Trim(),
                Category = values.ElementAt(SpreadsheetConstants.Transactions.Column.Category.LetterToColumnIndex()).ToString().Trim()
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

    public async Task<ExpenseResponse> GetLastExpenseAsync(string spreadsheetId)
    {

        var transactionsSheet = SpreadsheetConstants.Transactions.SheetName;
        logger.LogInformation("Removing expense process in spreadsheet '{SpreadsheetId}'.", spreadsheetId);

        try
        {
            var sheetId = await sheetsAccessor.GetSheetIdByNameAsync(spreadsheetId, transactionsSheet);

            var lastRow = await sheetsAccessor.FindLastItemAsync(
                spreadsheetId, transactionsSheet, SpreadsheetConstants.Transactions.Column.Description, SpreadsheetConstants.Transactions.DataStartRow);

            if (lastRow < SpreadsheetConstants.Transactions.DataStartRow)
            {
                logger.LogWarning("No expenses found in spreadsheet '{SpreadsheetId}'.", transactionsSheet);
                throw new InvalidOperationException("No expense found.");
            }

            var values = await sheetsAccessor.ReadRowValuesAsync(spreadsheetId, transactionsSheet, lastRow);

            if (values == null || values.Count == 0)
            {
                logger.LogWarning("Empty or invalid row");
                throw new InvalidOperationException("Could not retrieve expense data.");
            }

            var expense = new Expense
            {
                Description = values.ElementAtOrDefault(SpreadsheetConstants.Transactions.Column.Description.LetterToColumnIndex()).ToString().Trim(),
                Amount = values.ElementAtOrDefault(SpreadsheetConstants.Transactions.Column.Amount.LetterToColumnIndex()).ToString().Trim(),
                Category = values.ElementAt(SpreadsheetConstants.Transactions.Column.Category.LetterToColumnIndex()).ToString().Trim()
            };

            return new ExpenseResponse
            {
                Success = true,
                expense = expense
            };

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get last logged expense.");
            throw;
        }
    }
}