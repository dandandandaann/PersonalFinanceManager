using System.Text.Json.Serialization;
using SharedLibrary.Telegram.Enums;

namespace SharedLibrary.Telegram;

/// <summary>This object represents a message.</summary>
public class Message
{
    /// <summary>Unique message identifier inside this chat. In specific instances (e.g., message containing a video sent to a big chat), the server might automatically schedule a message instead of sending it immediately. In such cases, this field will be 0 and the relevant message will be unusable until it is actually sent</summary>
    [JsonPropertyName("message_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int Id { get; set; }

    /// <summary><em>Optional</em>. Sender of the message; may be empty for messages sent to channels. For backward compatibility, if the message was sent on behalf of a chat, the field contains a fake sender user in non-channel chats</summary>
    public User? From { get; set; }

    /// <summary>Date the message was sent. It is always a valid date.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public DateTime Date { get; set; }

    /// <summary>Chat the message belongs to</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public Chat Chat { get; set; } = default!;

    /// <summary><em>Optional</em>. For text messages, the actual text of the message</summary>
    public string? Text { get; set; }

    /// <summary><em>Optional</em>. For text messages, special entities like usernames, URLs, bot commands, etc. that appear in the text</summary>
    public MessageEntity[]? Entities { get; set; }

    /// <summary>Gets the <see cref="MessageType">type</see> of the <see cref="Message"/></summary>
    /// <value>The <see cref="MessageType">type</see> of the <see cref="Message"/></value>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public MessageType Type  { get; set; }
}