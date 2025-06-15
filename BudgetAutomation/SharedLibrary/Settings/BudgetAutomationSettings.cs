namespace SharedLibrary.Settings;

public class BudgetAutomationSettings
{
    public const string Configuration = "BudgetAutomation";

    public SpreadsheetManagerApiClientSettings SpreadsheetManagerApiClientSettings { get; set; } = null!;
    public SpreadsheetManagerApiSettings SpreadsheetManagerApiSettings { get; set; } = null!;
    public UserApiClientSettings userApiClientSettings { get; set; } = null!;
}