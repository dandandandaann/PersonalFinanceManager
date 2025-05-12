using System.Text.Json.Serialization;

namespace SharedLibrary.Telegram;

/// <summary>This object represents a chat.</summary>
public partial class Chat
{
    /// <summary>Unique identifier for this chat.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public long Id { get; set; }

    /// <summary>Type of the chat, can be either <see cref="ChatType.Private">Private</see>, <see cref="ChatType.Group">Group</see>, <see cref="ChatType.Supergroup">Supergroup</see> or <see cref="ChatType.Channel">Channel</see></summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public ChatType Type { get; set; }

    /// <summary><em>Optional</em>. Username, for private chats, supergroups and channels if available</summary>
    public string? Username { get; set; }
}