using ExpenseLoggerApi.Interface;
using SharedLibrary.Dto;
using SharedLibrary.Model;

namespace ExpenseLoggerApi.Service
{
    public class RemoveLoggerService(
        ISheetsDataAccessor sheetsAccessor,
        ILogger<RemoveLoggerService> logger)
    {
        public async Task<RemoveExpenseResponse> RemoveLastExpense(string spreadsheetId)
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

                if (lastRow < startRow)
                {
                    logger.LogWarning("No expenses found to remove in sheet '{SheetName}' in spreadsheet '{SpreadsheetId}'.",
                        sheetName, spreadsheetId);
                    throw new InvalidOperationException("Nenhuma despesa encontrada para remoção.");
                }

                var values = await sheetsAccessor.ReadRowValuesAsync(spreadsheetId, sheetName, lastRow);

                if (values == null || values.Count == 0)
                {
                    logger.LogWarning("Linha vazia ou inválida para remoção");
                    throw new InvalidOperationException("Não foi possível recuperar os dados da despesa para remoção.");
                }

                // Apaga a linha só depois de garantir os dados
                await sheetsAccessor.DeleteRowAsync(spreadsheetId, sheetId, lastRow);

                var description = values.ElementAtOrDefault(0)?.ToString().Trim();
                var amount = values.ElementAtOrDefault(3)?.ToString().Trim();
                var category = values.ElementAtOrDefault(7)?.ToString().Trim();

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
}
