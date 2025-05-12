namespace SharedLibrary.Telegram;

/// <summary>The type of <see cref="Message"/></summary>
public enum MessageType
{
    /// <summary><see cref="Message"/> type is unknown</summary>
    Unknown = 0,
    /// <summary>The <see cref="Message"/> contains a <see cref="Message.Text"/></summary>
    Text = 1,
    /// <summary>The <see cref="Message"/> contains a <see cref="Message.Photo"/></summary>
    Photo = 2,
    /// <summary>The <see cref="Message"/> contains an <see cref="Message.Audio"/></summary>
    Audio = 3,
    /// <summary>The <see cref="Message"/> contains a <see cref="Message.Video"/></summary>
    Video = 4,
    /// <summary>The <see cref="Message"/> contains a <see cref="Message.Voice"/></summary>
    Voice = 5,
    /// <summary>The <see cref="Message"/> contains a <see cref="Message.Document"/></summary>
    Document = 6,
    /// <summary>The <see cref="Message"/> contains a <see cref="Message.Sticker"/></summary>
    Sticker = 7,
    /// <summary>The <see cref="Message"/> contains a <see cref="Message.Location"/></summary>
    Location = 8,
    /// <summary>The <see cref="Message"/> contains a <see cref="Message.Contact"/></summary>
    Contact = 9,
    /// <summary>The <see cref="Message"/> contains a <see cref="Message.NewChatTitle"/></summary>
    NewChatTitle = 18
}