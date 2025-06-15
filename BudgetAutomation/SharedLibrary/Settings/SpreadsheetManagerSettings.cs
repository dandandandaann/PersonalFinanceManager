using SharedLibrary.Model;

namespace SharedLibrary.Settings;

public class SpreadsheetManagerSettings
{
    public const string Configuration = "SpreadsheetManager";

    public string credentials { get; set; } = string.Empty;
    public string googleApiKey { get; set; } = string.Empty;
    public string maxDailyRequest { get; set; } = string.Empty;
}