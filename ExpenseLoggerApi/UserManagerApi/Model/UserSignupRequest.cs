using System.Text.Json.Serialization;

namespace UserManagerApi.Model
{
    public class UserSignupRequest
    {
        public long TelegramId { get; set; }

        public string? Username { get; set; }

        // Parameterless constructor might be needed by the serializer
        public UserSignupRequest() { }

        public UserSignupRequest(long telegramId, string? username = null)
        {
            TelegramId = telegramId;
            Username = username;
        }
    }
}