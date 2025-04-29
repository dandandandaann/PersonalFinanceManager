namespace SharedLibrary.Settings;

public class ExpenseLoggerSettings
{
    public const string Configuration = "ExpenseLogger";

    public string credentials { get; set; } = string.Empty;
    public string googleApiKey { get; set; } = string.Empty;
    public string spreadsheetId { get; set; } = string.Empty;
    public string maxDailyRequest { get; set; } = string.Empty;
    public Category[]? Categories { get; set; }
}