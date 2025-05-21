using System.Text.Json.Serialization;

namespace SharedLibrary.Dto;

public class UserGetResponse : ApiResponse
{
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    [JsonPropertyName("user_configuration")]
    public UserConfigurationDto? userConfiguration { get; set; }
}