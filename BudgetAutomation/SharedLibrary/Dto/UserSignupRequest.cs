using System.Text.Json.Serialization;

namespace SharedLibrary.Dto;

public class UserSignupRequest(long telegramId, string email, string? username = null)
{
    [JsonPropertyName("telegram_id")]
    public long TelegramId { get; set; } = telegramId;

    [JsonPropertyName("username")]
    public string? Username { get; set; } = username;

    [JsonPropertyName("email")]
    public string Email { get; set; } = email;
}