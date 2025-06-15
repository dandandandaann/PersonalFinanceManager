namespace SharedLibrary.Settings;

public class SpreadsheetManagerApiSettings
{
    public const string Configuration = "SpreadsheetManagerApi";

    public string credentials { get; set; } = string.Empty;
    public string googleApiKey { get; set; } = string.Empty;
    public string maxDailyRequest { get; set; } = string.Empty;
}