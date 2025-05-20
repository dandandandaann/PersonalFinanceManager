using System.Text.Json.Serialization;

namespace SharedLibrary.Dto;

public class UserConfigurationResponse
{

    [JsonPropertyName("spreadsheetId")]
    public string SpreadsheetId { get; set; }
}