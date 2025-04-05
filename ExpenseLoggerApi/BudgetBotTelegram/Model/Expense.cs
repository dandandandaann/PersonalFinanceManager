namespace BudgetBotTelegram.Model
{
    public class Expense
    {
        public string Description { get; init; } = string.Empty;
        public string Amount { get; init; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        public override string ToString()
        {
            var categoryString = string.IsNullOrEmpty(Category) ? string.Empty : $", Category: {Category}";
            return $"Description: {Description}, Amount: {Amount}{categoryString}";
        }
    }
} 