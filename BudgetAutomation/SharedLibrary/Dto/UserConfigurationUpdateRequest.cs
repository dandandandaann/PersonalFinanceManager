using System.Text.Json.Serialization;
using SharedLibrary.Model;

namespace SharedLibrary.Dto;

public class UserConfigurationUpdateRequest(string userId, UserConfiguration userConfiguration)
{
    [JsonPropertyName("user_id")]
    public string Username { get; set; } = userId;

    [JsonPropertyName("user_configuration")]
    public UserConfiguration UserConfiguration { get; set; } = userConfiguration;
}