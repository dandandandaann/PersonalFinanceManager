using System.Text.Json.Serialization;
using SharedLibrary.Model;

namespace SharedLibrary.Dto;

public class ExpenseResponse : ApiResponse
{
    [JsonPropertyName("expense")]
    public Expense? expense { get; set; }
}