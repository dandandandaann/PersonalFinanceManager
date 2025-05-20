using System.Text.Json.Serialization;
using SharedLibrary.Interface;
using SharedLibrary.Model;

namespace SharedLibrary.Dto;

public class UserSignupResponse : IApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("user")]
    public User? User { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}