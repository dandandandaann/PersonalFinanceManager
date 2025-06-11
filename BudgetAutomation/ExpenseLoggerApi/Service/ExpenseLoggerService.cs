using System.Globalization;
using ExpenseLoggerApi.Constants;
using ExpenseLoggerApi.Interface;
using ExpenseLoggerApi.Misc;
using Google.Apis.Sheets.v4.Data;
using SharedLibrary.Model;

namespace ExpenseLoggerApi.Service;

public class ExpenseLoggerService(
    ISheetsDataAccessor sheetsAccessor,
    IEnumerable<Category> categories,
    ILogger<ExpenseLoggerService> logger)
{
    public async Task<Expense> LogExpense(string spreadsheetId, string description, string amount, string categoryInput)
    {
        var expense = new Expense
        {
            Description = description,
            Category = Utility.DecideCategory(categoryInput, description, categories)
        };

        // Use CultureInfo.InvariantCulture for reliable decimal parsing
        if (!double.TryParse(amount.Replace(',', '.'), CultureInfo.InvariantCulture, out var doubleAmount))
        {
            logger.LogError("Invalid amount format received: '{Amount}'.", amount);
            throw new ArgumentException("Invalid amount format.", nameof(amount));
        }

        doubleAmount = Math.Round(doubleAmount, 2);

        // Parse amount manually to pt-BR to send back in the response
        expense.Amount = doubleAmount.ToString("0.00", CultureInfo.InvariantCulture).Replace(",", "").Replace(".", ",");

        var sheetName = SpreadsheetConstants.Sheets.Transactions;
        logger.LogInformation("Starting expense logging process in spreadsheet '{SpreadsheetId}'.", spreadsheetId);

        try
        {
            var sheetId = await sheetsAccessor.GetSheetIdByNameAsync(spreadsheetId, sheetName);

            var row = await sheetsAccessor.FindFirstEmptyRowAsync(
                spreadsheetId, sheetName, SpreadsheetConstants.Column.Description, SpreadsheetConstants.DataStartRow
            );

            await sheetsAccessor.InsertRowAsync(spreadsheetId, sheetId, row);

            List<ValueRange> updates =
            [
                new()
                {
                    Range = $"{sheetName}!{SpreadsheetConstants.Column.Description}{row}", Values = Value(expense.Description)
                },
                new()
                {
                    Range = $"{sheetName}!{SpreadsheetConstants.Column.Category}{row}", Values = Value(expense.Category)
                },
                new()
                {
                    // let spreadsheet format the number
                    Range = $"{sheetName}!{SpreadsheetConstants.Column.Amount}{row}", Values = Value(doubleAmount)
                },
                new()
                {
                    Range = $"{sheetName}!{SpreadsheetConstants.Column.TotalFormula}{row}",
                    Values = Value(
                        $"=IF(ISBLANK({SpreadsheetConstants.Column.Amount}{row}); 0; " +
                        $"IF(ISBLANK({SpreadsheetConstants.Column.ExchangeRate}{row}); {SpreadsheetConstants.Column.Amount}{row}; " +
                        $"{SpreadsheetConstants.Column.Amount}{row}*{SpreadsheetConstants.Column.ExchangeRate}{row}))")
                },
                new()
                {
                    Range = $"{sheetName}!{SpreadsheetConstants.Column.Date}{row}",
                    Values = Value(DateTime.UtcNow.AddHours(SpreadsheetConstants.DateTimeZone).Date.ToOADate())
                },
                new()
                {
                    Range = $"{sheetName}!{SpreadsheetConstants.Column.DateCreated}{row}",
                    Values = Value(DateTime.UtcNow.AddHours(SpreadsheetConstants.DateTimeZone).ToOADate())
                },
                new()
                {
                    Range = $"{sheetName}!{SpreadsheetConstants.Column.Source}{row}", Values = Value("Telegram")
                }
            ];

            var requestBody = new BatchUpdateValuesRequest
            {
                Data = updates,
                // Use USER_ENTERED to allow formula parsing and number formatting based on locale
                ValueInputOption = "USER_ENTERED"
            };

            await sheetsAccessor.BatchUpdateValuesAsync(spreadsheetId, requestBody);

            return expense;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log expense for description '{Description}'.", description);
            throw;
        }
    }

    // Simple static helper to wrap value in the required list structure
    private static List<IList<object>> Value(object value) => [[value]];
}