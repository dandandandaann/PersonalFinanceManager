using System.Text.Json.Serialization;

namespace SharedLibrary.Dto;

public class UserConfigurationResponse
{
    [JsonPropertyName("spreadsheet_id")]
    public string SpreadsheetId { get; set; }
}