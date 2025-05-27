using ExpenseLoggerApi.Interface;
using SharedLibrary.Dto;
using SharedLibrary.Model;

namespace ExpenseLoggerApi.Service
{
    public class SpreadsheetService(
        ISheetsDataAccessor sheetsAccessor)
    {
        public async Task<bool> ValidateSpreadsheetId(string spreadsheetId)
        {

            var request = new SpreadsheetValidatorRequest { SpreadsheetId = spreadsheetId };
            var response = await sheetsAccessor.ValidateSpreadsheetIdAsync(request);

            if (!response.Success)
            {
                throw new UnauthorizedAccessException("Invalid Spreadsheet.");
            }

            return true;
        }
    }
}
