namespace SharedLibrary.Telegram;

public enum UpdateType
{
    /// <summary><see cref="Update"/> type is unknown</summary>
    Unknown = 0,
    /// <summary>The <see cref="Update"/> contains a <see cref="Update.Message"/></summary>
    Message = 1,
    /// <summary>The <see cref="Update"/> contains an <see cref="Update.InlineQuery"/></summary>
    InlineQuery = 2,
    /// <summary>The <see cref="Update"/> contains a <see cref="Update.ChosenInlineResult"/></summary>
    ChosenInlineResult = 3,
    /// <summary>The <see cref="Update"/> contains a <see cref="Update.CallbackQuery"/></summary>
    CallbackQuery = 4,
    /// <summary>The <see cref="Update"/> contains an <see cref="Update.EditedMessage"/></summary>
    EditedMessage = 5,
    /// <summary>The <see cref="Update"/> contains a <see cref="Update.MyChatMember"/></summary>
    MyChatMember = 12,
    /// <summary>The <see cref="Update"/> contains a <see cref="Update.ChatMember"/></summary>
    ChatMember = 13,
    /// <summary>The <see cref="Update"/> contains a <see cref="Update.ChatJoinRequest"/></summary>
    ChatJoinRequest = 14,
    /// <summary>The <see cref="Update"/> contains a <see cref="Update.MessageReaction"/></summary>
    MessageReaction = 15,
    /// <summary>The <see cref="Update"/> contains a <see cref="Update.MessageReactionCount"/></summary>
    MessageReactionCount = 16,
}