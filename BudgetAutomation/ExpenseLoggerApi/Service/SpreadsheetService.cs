using ExpenseLoggerApi.Constants;
using ExpenseLoggerApi.Interface;
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
        var sheetName = DateTime.Now.ToString("MM-yyyy");
        logger.LogInformation("Removing expense process for sheet '{SheetName}' in spreadsheet '{SpreadsheetId}'.",
            sheetName, spreadsheetId);

        try
        {
            var sheetId = await sheetsAccessor.GetSheetIdByNameAsync(spreadsheetId, sheetName);

            var lastRow = await sheetsAccessor.FindLastItemAsync(
                spreadsheetId, sheetName, SpreadsheetConstants.Column.Description, SpreadsheetConstants.DataStartRow);

            if (lastRow < SpreadsheetConstants.DataStartRow)
            {
                logger.LogWarning("No expenses found to remove in sheet '{SheetName}' in spreadsheet '{SpreadsheetId}'.",
                    sheetName, spreadsheetId);
                throw new InvalidOperationException("No expense found.");
            }

            var values = await sheetsAccessor.ReadRowValuesAsync(spreadsheetId, sheetName, lastRow);

            if (values == null || values.Count == 0)
            {
                logger.LogWarning("Empty or invalid row");
                throw new InvalidOperationException("Could not retrieve expense data.");
            }

            // Apaga a linha só depois de garantir os dados
            await sheetsAccessor.DeleteRowAsync(spreadsheetId, sheetId, lastRow);

            var description = values.ElementAtOrDefault((int)ExpenseColumnIndex.Description)?.ToString().Trim();
            var amount = values.ElementAtOrDefault((int)ExpenseColumnIndex.Amount)?.ToString().Trim();
            var category = values.ElementAtOrDefault((int)ExpenseColumnIndex.Category)?.ToString().Trim();

            var expense = new Expense
            {
                Description = description,
                Amount = amount,
                Category = category
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