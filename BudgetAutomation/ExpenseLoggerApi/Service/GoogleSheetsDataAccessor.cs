using ExpenseLoggerApi.Interface;
using Google;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using SharedLibrary.Dto;
using SharedLibrary.Enum;

namespace ExpenseLoggerApi.Service;

public class GoogleSheetsDataAccessor(SheetsService sheetsService, ILogger<GoogleSheetsDataAccessor> logger) : ISheetsDataAccessor
{
    public async Task<int> GetSheetIdByNameAsync(string spreadsheetId, string sheetName)
    {
        var request = sheetsService.Spreadsheets.Get(spreadsheetId);

        request.Fields = "sheets(properties(title,sheetId))"; // Request only titles and sheetIds within sheets
        var spreadsheet = await request.ExecuteAsync();

        var sheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName);

        if(sheet == null)
        {
            var templateSheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == "Template");

            var duplicateRequest = new DuplicateSheetRequest
            {
                SourceSheetId = templateSheet?.Properties.SheetId,
                NewSheetName = sheetName
            };

            var duplicateSheetRequest = new Request { DuplicateSheet = duplicateRequest };
            var batchUpdateRequest = new BatchUpdateSpreadsheetRequest
            {
                Requests = new List<Request> { duplicateSheetRequest }
            };

            var response = await sheetsService.Spreadsheets
                .BatchUpdate(batchUpdateRequest, spreadsheetId)
                .ExecuteAsync();

            return response.Replies.First().DuplicateSheet.Properties.SheetId.Value;
        }
        return sheet.Properties.SheetId.Value;
    }

    public async Task<int> FindFirstEmptyRowAsync(string spreadsheetId, string sheetName, string column, int startRow)
    {
        var range = $"{sheetName}!{column}{startRow}:{column}"; // Search column B from row 15 downward
        var request = sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);

        var response = await request.ExecuteAsync();
        var values = response.Values;

        if (values == null) return startRow;

        for (var i = 0; i < values.Count; i++)
        {
            if (values[i].Count == 0 || string.IsNullOrWhiteSpace(values[i][0].ToString()))
            {
                return startRow + i;
            }
        }

        return startRow + values.Count;
    }

    public async Task InsertRowAsync(string spreadsheetId, int sheetId, int rowIndex)
    {
        var requestBody = new Request
        {
            InsertDimension = new InsertDimensionRequest
            {
                Range = new DimensionRange
                {
                    SheetId = sheetId,
                    Dimension = "ROWS",
                    StartIndex = rowIndex - 1,
                    EndIndex = rowIndex
                }
            }
        };

        var batchRequest = new BatchUpdateSpreadsheetRequest { Requests = new List<Request> { requestBody } };
        var request = sheetsService.Spreadsheets.BatchUpdate(batchRequest, spreadsheetId);
        await request.ExecuteAsync();
    }

    public async Task BatchUpdateValuesAsync(string spreadsheetId, BatchUpdateValuesRequest request)
    {
        logger.LogDebug("Executing BatchUpdateValues for spreadsheet {SpreadsheetId}.", spreadsheetId);
        var updateRequest = sheetsService.Spreadsheets.Values.BatchUpdate(request, spreadsheetId);
        try
        {
            await updateRequest.ExecuteAsync();
            logger.LogDebug("BatchUpdateValues executed successfully.");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error executing BatchUpdateValues for spreadsheet {SpreadsheetId}.", spreadsheetId);
            throw;
        }
    }
    public async Task<SpreadsheetValidationResponse> ValidateSpreadsheetIdAsync(SpreadsheetValidationRequest request)
    {
        var response = new SpreadsheetValidationResponse();

        if (string.IsNullOrWhiteSpace(request.SpreadsheetId))
        {
            response.Success = false;
            response.Message = "O Id da planilha está vazio ou nulo";
            response.ErrorCode = ErrorCodeEnum.InvalidInput;
            return response;
        }

        try
        {
            var spreadsheetGet = sheetsService.Spreadsheets.Get(request.SpreadsheetId);
            var spreadsheet = await spreadsheetGet.ExecuteAsync();

            response.Success = spreadsheet?.SpreadsheetId == request.SpreadsheetId;
            response.Message = response.Success ? "Planilha válida." : "Id da planilha não corresponde.";

            return response;
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            response.Success = false;
            response.Message = "Planilha não encontrada";
            response.ErrorCode = ErrorCodeEnum.ResourceNotFound;
            return response;

        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = $"Erro ao validar a planilha: {ex.Message}";
            response.ErrorCode = ErrorCodeEnum.UnknownError;
            return response;
        }
    }
}