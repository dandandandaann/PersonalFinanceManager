using SharedLibrary.Model;
using System.Text.Json.Serialization;

namespace SharedLibrary.Dto;

public class RemoveExpenseResponse : ApiResponse
{
    [JsonPropertyName("expense")]
    public Expense? expense { get; set; }
}
