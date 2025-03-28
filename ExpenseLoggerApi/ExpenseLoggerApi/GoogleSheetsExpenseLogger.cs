using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace ExpenseLoggerApi;

class GoogleSheetsExpenseLogger
{
    private readonly string[] _scopes = [SheetsService.Scope.Spreadsheets];
    private const string ApplicationName = "Expense Logger";
    private readonly SheetsService _service;
    private readonly string _spreadsheetId;

    private readonly string[] _categories = // TODO: get category from secrets?
        ["Passagem", "Locomoção", "Hospedagem", "Mercado", "Comida", "Fun", "Outros"];

    public GoogleSheetsExpenseLogger(string credentialsJson, string spreadsheetId)
    {
        _spreadsheetId = spreadsheetId;
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(credentialsJson));
        var credential = GoogleCredential.FromStream(stream).CreateScoped(_scopes);

        _service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
    }

    public async Task LogExpense(string description, string amount, string category)
    {
        const int startRow = 15; // Starting row for expenses
        const string searchColumn = "B"; // 2nd column

        string sheetName = DateTime.Now.ToString("MM-yyyy");

        if (!double.TryParse(amount.Replace(',', '.'), out double doubleAmount))
            throw new ArgumentException("Invalid amount");
        doubleAmount = Math.Round(doubleAmount, 2);

        if (!_categories.Contains(category))
            category = "";

        int sheetId = GetSheetId(_spreadsheetId, sheetName);

        var row = await FindLastExpenseRow(sheetName, startRow, searchColumn);

        // Insert a new row at the found position
        await InsertRow(row, sheetId);

        // Prepare data updates
        List<ValueRange> updates =
        [
            new() { Range = $"{sheetName}!B{row}", Values = Value(description) },
            new() { Range = $"{sheetName}!E{row}", Values = Value(category)},
            new() { Range = $"{sheetName}!H{row}", Values = Value(doubleAmount) },
            new()
            {
                Range = $"{sheetName}!I{row}",
                Values = Value($"=IF(ISBLANK(H{row}); 0; IF(ISBLANK(F{row}); H{row}; F{row}*H{row}))")
            }
        ];

        var requestBody = new BatchUpdateValuesRequest
        {
            Data = updates,
            ValueInputOption = "USER_ENTERED"
        };

        var updateRequest = _service.Spreadsheets.Values.BatchUpdate(requestBody, _spreadsheetId);
        await updateRequest.ExecuteAsync();

        Console.WriteLine($"Expense logged successfully in row {row}!");
    }

    private static List<IList<object>> Value(object value) => [new List<object> { value }];

    private async Task<int> FindLastExpenseRow(string sheetName, int startRow, string column)
    {
        var range = $"{sheetName}!{column}{startRow}:{column}"; // Search column B from row 15 downward
        var request = _service.Spreadsheets.Values.Get(_spreadsheetId, range);
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

    private async Task InsertRow(int row, int sheetId)
    {
        var requestBody = new Request
        {
            InsertDimension = new InsertDimensionRequest
            {
                Range = new DimensionRange
                {
                    SheetId = sheetId,
                    Dimension = "ROWS",
                    StartIndex = row - 1,
                    EndIndex = row
                }
            }
        };

        var batchRequest = new BatchUpdateSpreadsheetRequest { Requests = new List<Request> { requestBody } };
        var request = _service.Spreadsheets.BatchUpdate(batchRequest, _spreadsheetId);
        await request.ExecuteAsync();
    }

    private int GetSheetId(string spreadsheetId, string sheetName)
    {
        var spreadsheet = _service.Spreadsheets.Get(spreadsheetId).Execute();
        var sheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName);
        return sheet?.Properties.SheetId ?? throw new Exception("Sheet not found!");
    }
}