namespace SharedLibrary.Telegram.Enums;

/// <summary>Type of <see cref="MessageEntity"/></summary>
public enum MessageEntityType
{
    /// <summary>A mentioned <see cref="User"/></summary>
    Mention = 1,
    /// <summary>A Bot command</summary>
    BotCommand = 3,
    /// <summary>An URL</summary>
    Url = 4,
    /// <summary>An email</summary>
    Email = 5,
}