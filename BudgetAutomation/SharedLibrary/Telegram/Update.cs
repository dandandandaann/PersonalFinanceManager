using System.Text.Json.Serialization;

namespace SharedLibrary.Telegram;

public partial class Update
{
    /// <summary>The update's unique identifier. Update identifiers start from a certain positive number and increase sequentially. This identifier becomes especially handy if you're using <see cref="TelegramBotClientExtensions.SetWebhook">webhooks</see>, since it allows you to ignore repeated updates or to restore the correct update sequence, should they get out of order. If there are no new updates for at least a week, then identifier of the next update will be chosen randomly instead of sequentially.</summary>
    [JsonPropertyName("update_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int Id { get; set; }

    /// <summary><em>Optional</em>. New incoming message of any kind - text, photo, sticker, etc.</summary>
    public Message? Message { get; set; }

    /// <summary>Gets the <see cref="UpdateType">type</see> of the <see cref="Update"/></summary>
    /// <value>The <see cref="UpdateType">type</see> of the <see cref="Update"/></value>
    public UpdateType Type;
}