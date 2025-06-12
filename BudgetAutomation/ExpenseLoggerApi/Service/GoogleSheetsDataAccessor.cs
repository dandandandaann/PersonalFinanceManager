using System.Net;
using ExpenseLoggerApi.Interface;
using ExpenseLoggerApi.Misc;
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

        Spreadsheet? spreadsheet;
        try
        {
            spreadsheet = await request.ExecuteAsync();
        }
        catch (GoogleApiException ex) when (ex.Error.Code == (int)HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException(ex.Error.Message);
        }
        catch (GoogleApiException ex) when (ex.Error.Code == (int)HttpStatusCode.NotFound)
        {
            spreadsheet = null;
        }

        if (spreadsheet is null || !spreadsheet.Sheets.Any())
            throw new SpreadsheetNotFoundException($"Spreadsheet id '{spreadsheetId}' not found.");

        var sheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName);

        if (sheet?.Properties.SheetId == null)
            throw new SheetNotFoundException($"Sheet '{sheetName}' not found in Spreadsheet id '{spreadsheetId}'.");

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

    public async Task<int> FindLastItemAsync(string spreadsheetId, string sheetName, string column, int startRow)
    {
        var lastItem = (await FindFirstEmptyRowAsync(spreadsheetId, sheetName, column, startRow)) - 1;

        if (lastItem < startRow)
        {
            logger.LogInformation("Nenhum item encontrado na coluna {Column} da planilha '{SheetName}'.", column, sheetName);
            throw new InvalidOperationException("No item found in column of the spreadsheet.");
        }

        return lastItem;
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

    public async Task DeleteRowAsync(string spreadsheetId, int sheetId, int rowIndex)
    {
        var requestBody = new Request
        {
            DeleteDimension = new DeleteDimensionRequest
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

    public async Task<IList<object>> ReadRowValuesAsync(string spreadsheetId, string sheetName, int rowIndex)
    {
        var range = $"{sheetName}!B{rowIndex}:I{rowIndex}";
        var request = sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);
        var response = await request.ExecuteAsync();

        var values = response.Values?.FirstOrDefault();

        return values;
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
        catch (Exception ex)
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