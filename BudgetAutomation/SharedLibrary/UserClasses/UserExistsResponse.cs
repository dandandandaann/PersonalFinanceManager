using System.Text.Json.Serialization;

namespace SharedLibrary.UserClasses;

public class UserExistsResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}