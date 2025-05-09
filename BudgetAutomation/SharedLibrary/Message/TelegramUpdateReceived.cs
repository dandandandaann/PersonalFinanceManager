using System.Text.Json.Serialization;

namespace SharedLibrary.Message;

public class TelegramUpdateReceived
{
    [JsonPropertyName("update")]
    public string Update { get; set; }
}