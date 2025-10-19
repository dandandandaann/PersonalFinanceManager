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
        [JsonPropertyName("exchangeRate")]
        public string ExchangeRate { get; set; } = string.Empty;

        public override string ToString()
        {
            var stringResult = $"Descrição: {Description} \n" +
                               $"Valor: {Amount}";
            stringResult += string.IsNullOrEmpty(Category) ? string.Empty : $" \nCategoria: {Category}";
            stringResult += string.IsNullOrEmpty(ExchangeRate) ? string.Empty : $" \nCotação: {ExchangeRate}";

            return stringResult;
        }
    }
} 