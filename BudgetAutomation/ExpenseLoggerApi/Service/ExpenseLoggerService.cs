using System.Globalization;
using ExpenseLoggerApi.Interface;
using Google.Apis.Sheets.v4.Data;
using SharedLibrary;
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
            Category = DecideCategory(categoryInput, description)
        };

        const int startRow = 15; // Starting row for expenses
        const string searchColumn = "B"; // 2nd column where expense descriptions are expected

        // Use CultureInfo.InvariantCulture for reliable decimal parsing
        if (!double.TryParse(amount.Replace(',', '.'), CultureInfo.InvariantCulture, out var doubleAmount))
        {
            logger.LogError("Invalid amount format received: '{Amount}'.", amount);
            throw new ArgumentException("Invalid amount format.", nameof(amount));
        }

        doubleAmount = Math.Round(doubleAmount, 2);

        // Parse amount manually to pt-BR
        expense.Amount = doubleAmount.ToString("0.00", CultureInfo.InvariantCulture).Replace(",", "").Replace(".", ",");

        var sheetName = DateTime.Now.ToString("MM-yyyy");
        logger.LogInformation("Starting expense logging process for sheet '{SheetName}' in spreadsheet '{SpreadsheetId}'.",
            sheetName, spreadsheetId);

        try
        {
            var sheetId = await sheetsAccessor.GetSheetIdByNameAsync(spreadsheetId, sheetName);

            var row = await sheetsAccessor.FindFirstEmptyRowAsync(spreadsheetId, sheetName, searchColumn, startRow);

            await sheetsAccessor.InsertRowAsync(spreadsheetId, sheetId, row);

            List<ValueRange> updates =
            [
                new() { Range = $"{sheetName}!B{row}", Values = Value(expense.Description) },
                new() { Range = $"{sheetName}!E{row}", Values = Value(expense.Category) },
                new() { Range = $"{sheetName}!H{row}", Values = Value(doubleAmount) }, // let spreadsheet format the number
                new() // Formula for amount calculation
                {
                    Range = $"{sheetName}!I{row}",
                    Values = Value($"=IF(ISBLANK(H{row}); 0; IF(ISBLANK(F{row}); H{row}; F{row}*H{row}))")
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

    private string DecideCategory(string userCategory, string description)
    {
        description = description.Trim().Normalize();

        if (string.IsNullOrEmpty(userCategory))
        {
            foreach (var category in categories)
            {
                if (category.Alias == null)
                    continue;

                if (category.Alias.Any(alias =>
                description.Contains(alias, StringComparison.OrdinalIgnoreCase)))
                {

                    return category.Name;
                }
            }
            return "";
        }

        foreach (var category in categories)
        {
            if (!string.IsNullOrEmpty(userCategory))
            {
                if (category.Name.Equals(userCategory, StringComparison.OrdinalIgnoreCase))
                {
                    return category.Name;
                }

                if (category.Alias != null && category.Alias.Any(alias =>
                    alias.Equals(userCategory, StringComparison.OrdinalIgnoreCase)))
                {
                    return category.Name;
                }
            }
        }
        return ""; // Return empty string if no match found
    }
}

