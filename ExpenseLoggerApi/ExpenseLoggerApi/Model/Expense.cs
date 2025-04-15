namespace ExpenseLoggerApi.Model
{
    public class Expense
    {
        public string Description { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Category { get; set; } = string.Empty;

        public override string ToString()
        {
            var categoryString = string.IsNullOrEmpty(Category) ? string.Empty : $"\nCategory: {Category}";
            return $"Description: {Description}\nAmount: {Amount}{categoryString}";
        }
    }
} 