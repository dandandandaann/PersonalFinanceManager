using System.Text.Json.Serialization;

namespace SharedLibrary.Telegram;

/// <summary>This object represents a Telegram user or bot.</summary>
public partial class User
{
    /// <summary>Unique identifier for this user or bot.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public long Id { get; set; }

    /// <summary><em>Optional</em>. User's or bot's username</summary>
    public string? Username { get; set; }
}