using System.Text.Json.Serialization;

namespace SharedLibrary.Telegram;

/// <summary>This object represents an incoming callback query from a callback button in an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>. If the button that originated the query was attached to a message sent by the bot, the field <see cref="P:Telegram.Bot.Types.CallbackQuery.Message">Message</see> will be present. If the button was attached to a message sent via the bot (in <a href="https://core.telegram.org/bots/api#inline-mode">inline mode</a>), the field <see cref="P:Telegram.Bot.Types.CallbackQuery.InlineMessageId">InlineMessageId</see> will be present. Exactly one of the fields <see cref="P:Telegram.Bot.Types.CallbackQuery.Data">Data</see> or <see cref="P:Telegram.Bot.Types.CallbackQuery.GameShortName">GameShortName</see> will be present.</summary>
/// <remarks><b>NOTE:</b> After the user presses a callback button, Telegram clients will display a progress bar until you call <see cref="M:Telegram.Bot.TelegramBotClientExtensions.AnswerCallbackQuery(Telegram.Bot.ITelegramBotClient,System.String,System.String,System.Boolean,System.String,System.Nullable{System.Int32},System.Threading.CancellationToken)">AnswerCallbackQuery</see>. It is, therefore, necessary to react by calling <see cref="M:Telegram.Bot.TelegramBotClientExtensions.AnswerCallbackQuery(Telegram.Bot.ITelegramBotClient,System.String,System.String,System.Boolean,System.String,System.Nullable{System.Int32},System.Threading.CancellationToken)">AnswerCallbackQuery</see> even if no notification to the user is needed (e.g., without specifying any of the optional parameters).</remarks>
public class CallbackQuery
{
    /// <summary>Unique identifier for this query</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string Id { get; set; } = null!;

    /// <summary>Sender</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public User From { get; set; } = null!;

    /// <summary><em>Optional</em>. Message sent by the bot with the callback button that originated the query</summary>
    public Message? Message { get; set; }

    /// <summary><em>Optional</em>. Identifier of the message sent via the bot in inline mode, that originated the query.</summary>
    [JsonPropertyName("inline_message_id")]
    public string? InlineMessageId { get; set; }

    /// <summary>Global identifier, uniquely corresponding to the chat to which the message with the callback button was sent. Useful for high scores in <a href="https://core.telegram.org/bots/api#games">games</a>.</summary>
    [JsonPropertyName("chat_instance")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string ChatInstance { get; set; } = null!;

    /// <summary><em>Optional</em>. Data associated with the callback button. Be aware that the message originated the query can contain no callback buttons with this data.</summary>
    public string? Data { get; set; }
}