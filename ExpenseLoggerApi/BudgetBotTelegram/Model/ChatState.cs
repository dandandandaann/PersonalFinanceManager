using Amazon.DynamoDBv2.DataModel;

namespace BudgetBotTelegram.Model
{
    [DynamoDBTable("chat_state")] // Ensure this matches your table name
    public class ChatState
    {
        private readonly string _chatId = string.Empty;

        // Partition Key
        [DynamoDBHashKey("pk")]
        public string Pk { get; set; }

        // Sort Key
        [DynamoDBRangeKey("sk")]
        public string Sk { get; set; }

        [DynamoDBProperty("telegramId")] // userId on telegram
        public string TelegramId { get; set; }

        [DynamoDBProperty("chatId")]
        public string ChatId
        {
            get => _chatId;
            init
            {
                _chatId = value;
                // Keep TelegramId, Pk and Sk synchronized with ChatId for now
                TelegramId = _chatId;
                Pk = _chatId;
                Sk = _chatId;
            }
        }

        [DynamoDBProperty("state")]
        public string? State { get; init; }

        [DynamoDBProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        // Parameterless constructor required by DynamoDBContext
        public ChatState() { }

        public ChatState(long chatId, string? state = null)
        {
            ChatId = chatId.ToString();
            State = state;
            Timestamp = DateTime.UtcNow;
        }
    }
}