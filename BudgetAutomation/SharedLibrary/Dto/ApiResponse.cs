using System.Text.Json.Serialization;
using SharedLibrary.Enum;

namespace SharedLibrary.Dto;

/// <summary>
/// Common class for response DTOs
/// </summary>
public class ApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = "";

    [JsonPropertyName("error_code")]
    public ErrorCodeEnum ErrorCode { get; set; } = ErrorCodeEnum.None;
}