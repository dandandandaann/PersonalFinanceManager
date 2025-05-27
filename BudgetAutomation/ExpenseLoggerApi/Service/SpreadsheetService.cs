using ExpenseLoggerApi.Interface;
using SharedLibrary.Dto;

namespace ExpenseLoggerApi.Service;

public class SpreadsheetService(ISheetsDataAccessor sheetsAccessor)
{
    public async Task<SpreadsheetValidationResponse> ValidateSpreadsheetId(string spreadsheetId)
    {
        var request = new SpreadsheetValidationRequest { SpreadsheetId = spreadsheetId };
        var response = await sheetsAccessor.ValidateSpreadsheetIdAsync(request);

        return response;
    }
}