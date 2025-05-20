using System.Text.Json.Serialization;
using SharedLibrary.Interface;
using SharedLibrary.Model;

namespace SharedLibrary.Dto;

public class LogExpenseResponse : IApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("expense")]
    public Expense? expense { get; set; }
}