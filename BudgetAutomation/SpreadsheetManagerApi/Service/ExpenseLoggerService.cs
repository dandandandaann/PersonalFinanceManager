using System.Globalization;
using SpreadsheetManagerApi.Interface;
using Google.Apis.Sheets.v4.Data;
using SharedLibrary.Constants;
using SharedLibrary.Model;

namespace SpreadsheetManagerApi.Service;

public class ExpenseLoggerService(
    ISheetsDataAccessor sheetsAccessor,
    ICategoryService categoryService,
    ILogger<ExpenseLoggerService> logger)
{
    public async Task<Expense> LogExpense(string spreadsheetId, string description, string amount, string categoryInput)
    {
        var expense = new Expense
        {
            Description = description,
            Category = await categoryService.DecideCategoryAsync(spreadsheetId, categoryInput, description)
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

        var sheetName = SpreadsheetConstants.Transactions.SheetName;
        logger.LogInformation("Starting expense logging process in spreadsheet '{SpreadsheetId}'.", spreadsheetId);

        try
        {
            var sheetId = await sheetsAccessor.GetSheetIdByNameAsync(spreadsheetId, sheetName);

            var row = await sheetsAccessor.FindFirstEmptyRowAsync(
                spreadsheetId, sheetName,
                SpreadsheetConstants.Transactions.Column.Description, SpreadsheetConstants.Transactions.DataStartRow
            );

            await sheetsAccessor.InsertRowAsync(spreadsheetId, sheetId, row);

            List<ValueRange> updates =
            [
                new()
                {
                    Range = $"{sheetName}!{SpreadsheetConstants.Transactions.Column.Description}{row}", Values = Value(expense.Description)
                },
                new()
                {
                    Range = $"{sheetName}!{SpreadsheetConstants.Transactions.Column.Category}{row}", Values = Value(expense.Category)
                },
                new()
                {
                    // let spreadsheet format the number
                    Range = $"{sheetName}!{SpreadsheetConstants.Transactions.Column.Amount}{row}", Values = Value(doubleAmount)
                },
                new()
                {
                    Range = $"{sheetName}!{SpreadsheetConstants.Transactions.Column.TotalFormula}{row}",
                    Values = Value(
                        $"=IF(ISBLANK({SpreadsheetConstants.Transactions.Column.Amount}{row}); 0; " +
                        $"IF(ISBLANK({SpreadsheetConstants.Transactions.Column.ExchangeRate}{row}); {SpreadsheetConstants.Transactions.Column.Amount}{row}; " +
                        $"{SpreadsheetConstants.Transactions.Column.Amount}{row}*{SpreadsheetConstants.Transactions.Column.ExchangeRate}{row}))")
                },
                new()
                {
                    Range = $"{sheetName}!{SpreadsheetConstants.Transactions.Column.Date}{row}",
                    Values = Value(DateTime.UtcNow.AddHours(SpreadsheetConstants.DateTimeZone).Date.ToOADate())
                },
                new()
                {
                    Range = $"{sheetName}!{SpreadsheetConstants.Transactions.Column.DateCreated}{row}",
                    Values = Value(DateTime.UtcNow.AddHours(SpreadsheetConstants.DateTimeZone).ToOADate())
                },
                new()
                {
                    Range = $"{sheetName}!{SpreadsheetConstants.Transactions.Column.Source}{row}", Values = Value("Telegram")
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