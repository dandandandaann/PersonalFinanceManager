using System.Net;
using System.Collections.Concurrent;
using SpreadsheetManagerApi.Interface;
using SpreadsheetManagerApi.Misc;
using Google;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using SharedLibrary.Constants;
using SharedLibrary.Dto;
using SharedLibrary.Enum;

namespace SpreadsheetManagerApi.Service;

public class GoogleSheetsDataAccessor(SheetsService sheetsService, ILogger<GoogleSheetsDataAccessor> logger) : ISheetsDataAccessor
{
    private readonly ConcurrentDictionary<string, int> _sheetIdCache = new();

    public async Task<int> GetSheetIdByNameAsync(string spreadsheetId, string sheetName)
    {
        var cacheKey = $"{spreadsheetId}:{sheetName}";
        if (_sheetIdCache.TryGetValue(cacheKey, out var cachedId))
            return cachedId;

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

        if (sheet?.Properties.SheetId is null or 0)
            throw new SheetNotFoundException($"Sheet '{sheetName}' not found in Spreadsheet id '{spreadsheetId}'.");

        var sheetId = sheet.Properties.SheetId.Value;
        _sheetIdCache.TryAdd(cacheKey, sheetId);
        return sheetId;
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
                },
                InheritFromBefore = true
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
        var range = $"{sheetName}!A{rowIndex}:O{rowIndex}";
        var request = sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);
        var response = await request.ExecuteAsync();

        var values = response.Values?.FirstOrDefault();

        return values;
    }

    public async Task<IList<string>> ReadColumnValuesAsync(string spreadsheetId, string sheetName, string column, int startRow)
    {
        var range = $"{sheetName}!{column}{startRow}:{column}";
        var request = sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);
        var response = await request.ExecuteAsync();

        if (response.Values == null)
            return new List<string>();

        return response.Values
            .Where(row => row.Count > 0 && !string.IsNullOrWhiteSpace(row[0]?.ToString()))
            .Select(row => row[0].ToString()!)
            .ToList();
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
        if (string.IsNullOrWhiteSpace(request.SpreadsheetId))
        {
            return new SpreadsheetValidationResponse
            {
                Success = false,
                Message = "Spreadsheet Id is null or empty.",
                ErrorCode = ErrorCodeEnum.InvalidInput,
            };
        }

        await GetSheetIdByNameAsync(request.SpreadsheetId, SpreadsheetConstants.Transactions.SheetName);

        return new SpreadsheetValidationResponse
        {
            Success = true,
            Message = "Empty spreadsheet."
        };
    }

    private async Task<int> GetSheetRowCountAsync(string spreadsheetId, string sheetName)
    {
        var request = sheetsService.Spreadsheets.Get(spreadsheetId);
        request.Fields = "sheets(properties(title,gridProperties(rowCount)))";

        var spreadsheet = await request.ExecuteAsync();
        var sheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName)
                   ?? throw new InvalidOperationException($"Sheet '{sheetName}' not found.");

        return sheet.Properties.GridProperties.RowCount ?? 0;
    }

    public async Task<int> FindFirstEmptyRowAsync(string spreadsheetId, string sheetName, string column)
    {
        const int searchWindow = 20;
        var rowCount = await GetSheetRowCountAsync(spreadsheetId, sheetName);
        if (rowCount <= 0) return 1;

        var startSearchRow = Math.Max(1, rowCount - searchWindow + 1);
        var endSearchRow = rowCount;

        ValueRange? resp;

        do
        {
            var range = $"{sheetName}!{column}{startSearchRow}:{column}{endSearchRow}";
            resp = await sheetsService.Spreadsheets.Values.Get(spreadsheetId, range).ExecuteAsync();

            startSearchRow = Math.Max(1, startSearchRow - searchWindow);
            endSearchRow = Math.Max(0, endSearchRow - searchWindow);

        } while (resp.Values == null && endSearchRow != 0);

        // If endRow gets to 1, the API only returned empty, which means the whole column is empty.
        if (resp.Values != null)
            return endSearchRow + resp.Values.Count + 1;

        return 1;

    }
}