using System.Text.Json.Serialization;

namespace SharedLibrary.Model
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
            var categoryString = string.IsNullOrEmpty(Category) ? string.Empty : $" \nCategoria: {Category}";
            return $"Descrição: {Description} \nValor: {Amount}{categoryString}";
        }
    }
} 