using System.Text.Json.Serialization;

namespace SharedLibrary.Dto;

public class UserConfigurationUpdateRequest(UserConfigurationDto userConfiguration)
{
    [JsonPropertyName("user_configuration")]
    public UserConfigurationDto UserConfiguration { get; set; } = userConfiguration;
}