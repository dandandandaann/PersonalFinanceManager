namespace SharedLibrary.Settings;

public class BudgetAutomationSettings
{
    public const string Configuration = "BudgetAutomation";

    public SpreadsheetManagerApiClientSettings SpreadsheetManagerApiClientSettings { get; set; } = null!;
    public SpreadsheetManagerSettings SpreadsheetManagerSettings { get; set; } = null!;
    public UserApiClientSettings userApiClientSettings { get; set; } = null!;
}