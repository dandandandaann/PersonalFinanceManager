using System.Text.Json.Serialization;

namespace SharedLibrary.Dto;

public class UserConfigurationDto
{
    [JsonPropertyName("spreadsheet_id")]
    public string SpreadsheetId { get; set; }
}