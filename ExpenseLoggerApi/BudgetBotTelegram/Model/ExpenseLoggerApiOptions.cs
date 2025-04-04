namespace BudgetBotTelegram.Model
{
    public class ExpenseLoggerApiOptions
    {
        public const string Configuration = "ExpenseLoggerApi";

        public string Url { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
    }
} 