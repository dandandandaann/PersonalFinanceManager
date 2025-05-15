using System.Text.Json.Serialization;

namespace SharedLibrary.UserClasses;

public class UserResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("user")]
    public User? User { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}