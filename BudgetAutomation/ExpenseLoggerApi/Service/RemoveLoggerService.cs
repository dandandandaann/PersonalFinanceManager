using ExpenseLoggerApi.Interface;
using SharedLibrary.Dto;
using SharedLibrary.Model;

namespace ExpenseLoggerApi.Service
{
    public class RemoveLoggerService(
        ISheetsDataAccessor sheetsAccessor,
        ILogger<RemoveLoggerService> logger)
    {
        public async Task<RemoveExpenseResponse> RemoveLastExpense(string spreadsheetId, string description, string amount, string category)
        {
           
            const int startRow = 15;
            const string searchColumn = "B";

            var sheetName = DateTime.Now.ToString("MM-yyyy");
            logger.LogInformation("Removing expense process for sheet '{SheetName}' in spreadsheet '{SpreadsheetId}'.",
                sheetName, spreadsheetId);

            try
            {
                var sheetId = await sheetsAccessor.GetSheetIdByNameAsync(spreadsheetId, sheetName);

                var lastRow = await sheetsAccessor.FindLastItemAsync(spreadsheetId, sheetName, searchColumn, startRow);

                if(lastRow < startRow)
                {
                    logger.LogWarning("No expenses found to remove in sheet '{SheetName}' in spreadsheet '{SpreadsheetId}'.",
                        sheetName, spreadsheetId);
                    throw new ArgumentException("Invalid row.", nameof(lastRow));
                }

                await sheetsAccessor.DeleteRowAsync(spreadsheetId, sheetId, lastRow);
                logger.LogInformation("Removed last logged expense at row {Row} from sheet '{SheetName}'.",
                    lastRow, sheetName);

                return new RemoveExpenseResponse
                {
                    expense = new Expense
                    {
                        Description = description,
                        Amount = amount,
                        Category = category
                    }
                };

            }catch(Exception ex)
            {
                logger.LogError(ex, "Failed to remove last logged expense for description '{Description}'.", description);
                throw;
            }
        }
    }
}
