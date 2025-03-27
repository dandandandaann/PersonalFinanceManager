using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace ExpenseLoggerApi;

class GoogleSheetsExpenseLogger
{
    private readonly string[] _scopes = [SheetsService.Scope.Spreadsheets];
    private const string ApplicationName = "Expense Logger";
    private const string SpreadsheetId = "10KLvA6_aK992hVNB0PC9grsrhIZdrr2SdBctAKvNiqM";
    private const string SheetName = "expenses";
    private readonly SheetsService _service;

    public GoogleSheetsExpenseLogger(string credentialsJson)
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(credentialsJson));
        var credential = GoogleCredential.FromStream(stream).CreateScoped(_scopes);

        _service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
    }

    public async Task LogExpense(string category, string description, double amount)
    {
        const int startRow = 15; // Starting row for expenses
        const int searchColumn = 2; // Column B (2nd column)
        var row = await FindLastExpenseRow(startRow, searchColumn);

        // Insert a new row at the found position
        await InsertRow(row);

        // Prepare data updates
        List<ValueRange> updates =
        [
            new ValueRange
            {
                Range = $"{SheetName}!B{row}", Values = new List<IList<object>> { new List<object> { description } }
            },

            new ValueRange
                { Range = $"{SheetName}!C{row}", Values = new List<IList<object>> { new List<object> { category } } },

            new ValueRange
                { Range = $"{SheetName}!H{row}", Values = new List<IList<object>> { new List<object> { amount } } },

            new ValueRange
            {
                Range = $"{SheetName}!I{row}",
                Values = new List<IList<object>>
                    { new List<object> { $"=IF(ISBLANK(H{row}); 0; IF(ISBLANK(F{row}); H{row}; F{row}*H{row}))" } }
            }
        ];

        var requestBody = new BatchUpdateValuesRequest
        {
            Data = updates,
            ValueInputOption = "USER_ENTERED"
        };

        var updateRequest = _service.Spreadsheets.Values.BatchUpdate(requestBody, SpreadsheetId);
        await updateRequest.ExecuteAsync();

        Console.WriteLine($"Expense logged successfully in row {row}!");
    }

    private async Task<int> FindLastExpenseRow(int startRow, int columnIndex)
    {
        var range = $"{SheetName}!B{startRow}:B"; // Search column B from row 15 downward
        var request = _service.Spreadsheets.Values.Get(SpreadsheetId, range);
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

    private async Task InsertRow(int row)
    {
        var requestBody = new Request
        {
            InsertDimension = new InsertDimensionRequest
            {
                Range = new DimensionRange
                {
                    SheetId = GetSheetId(),
                    Dimension = "ROWS",
                    StartIndex = row - 1,
                    EndIndex = row
                }
            }
        };

        var batchRequest = new BatchUpdateSpreadsheetRequest { Requests = new List<Request> { requestBody } };
        var request = _service.Spreadsheets.BatchUpdate(batchRequest, SpreadsheetId);
        await request.ExecuteAsync();
    }

    private int GetSheetId()
    {
        var spreadsheet = _service.Spreadsheets.Get(SpreadsheetId).Execute();
        var sheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == SheetName);
        return sheet?.Properties.SheetId ?? throw new Exception("Sheet not found!");
    }
}