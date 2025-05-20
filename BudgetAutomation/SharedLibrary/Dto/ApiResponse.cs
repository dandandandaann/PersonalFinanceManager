using System.Text.Json.Serialization;

namespace SharedLibrary.Dto;

/// <summary>
/// Common class for response DTOs
/// </summary>
public class ApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}