using System.Text.Json.Serialization;

namespace SharedLibrary;

public class LogExpenseResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("expense")]
    public Expense? expense { get; set; }
}