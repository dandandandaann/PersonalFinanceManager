using System.Text.Json.Serialization;

namespace SharedLibrary
{
    public class Expense
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("amount")]
        public string Amount { get; set; } = string.Empty;
        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        public override string ToString()
        {
            var categoryString = string.IsNullOrEmpty(Category) ? string.Empty : $" \nCategory: {Category}";
            return $"Description: {Description} \nAmount: {Amount}{categoryString}";
        }
    }
} 