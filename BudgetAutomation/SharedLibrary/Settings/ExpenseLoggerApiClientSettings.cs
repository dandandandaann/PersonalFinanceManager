namespace SharedLibrary.Settings
{
    public class ExpenseLoggerApiClientSettings
    {
        public const string Configuration = "ExpenseLoggerApiClient";

        public string Url { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
    }
} 