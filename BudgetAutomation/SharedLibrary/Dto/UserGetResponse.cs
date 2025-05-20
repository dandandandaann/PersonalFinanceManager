using System.Text.Json.Serialization;
using SharedLibrary.Interface;

namespace SharedLibrary.Dto;

public class UserGetResponse : IApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("userConfiguration")]
    public UserConfigurationResponse userConfiguration { get; set; }
}