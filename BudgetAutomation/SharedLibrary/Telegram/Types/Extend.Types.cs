using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0290 // Use primary constructor

namespace SharedLibrary.Telegram.Types
{
    namespace ReplyMarkups
    {
        public partial class ReplyKeyboardMarkup
        {
            /// <summary>Initializes a new instance of <see cref="ReplyKeyboardMarkup"/> with one button</summary>
            /// <param name="button">Button or text on keyboard</param>
            [SetsRequiredMembers]
            public ReplyKeyboardMarkup(KeyboardButton button) : this(new List<List<KeyboardButton>> { new() { button } }) { }

            /// <summary>Initializes a new instance of <see cref="ReplyKeyboardMarkup"/></summary>
            /// <param name="keyboardRow">The keyboard row.</param>
            [SetsRequiredMembers]
            public ReplyKeyboardMarkup(IEnumerable<KeyboardButton> keyboardRow) : this(new List<List<KeyboardButton>> { keyboardRow.ToList() }) { }

#pragma warning disable MA0016 // Prefer using collection abstraction instead of implementation
            /// <summary>Initializes a new instance of <see cref="ReplyKeyboardMarkup"/></summary>
            /// <param name="keyboardRow">The keyboard row.</param>
            [SetsRequiredMembers]
            public ReplyKeyboardMarkup(List<KeyboardButton> keyboardRow) : this(new List<List<KeyboardButton>> { keyboardRow }) { }
#pragma warning restore MA0016 // Prefer using collection abstraction instead of implementation

            /// <summary>Initializes a new instance of <see cref="ReplyKeyboardMarkup"/></summary>
            /// <param name="keyboardRow">A row of buttons or texts.</param>
            [SetsRequiredMembers]
            public ReplyKeyboardMarkup(params KeyboardButton[] keyboardRow) : this(new List<List<KeyboardButton>> { keyboardRow.ToList() }) { }

            /// <summary>Instantiates a new <see cref="ReplyKeyboardMarkup"/></summary>
            [SetsRequiredMembers]
            public ReplyKeyboardMarkup(bool resizeKeyboard) : this() => ResizeKeyboard = resizeKeyboard;

            /// <summary>Generates a reply keyboard markup with one button</summary>
            /// <param name="text">Button's text</param>
            [return: NotNullIfNotNull(nameof(text))]
            public static implicit operator ReplyKeyboardMarkup?(string? text) => text is null ? default : new(text);

            /// <summary>Generates a reply keyboard markup with multiple buttons on one row</summary>
            /// <param name="texts">Texts of buttons</param>
            [return: NotNullIfNotNull(nameof(texts))]
            public static implicit operator ReplyKeyboardMarkup?(string[]? texts) => texts is null ? default : new[] { texts };

            /// <summary>Generates a reply keyboard markup with multiple buttons</summary>
            /// <param name="texts">Texts of buttons</param>
            [return: NotNullIfNotNull(nameof(texts))]
            public static implicit operator ReplyKeyboardMarkup?(string[][]? texts) => texts is null ? default
                : new ReplyKeyboardMarkup(texts.Select(texts => texts.Select(t => new KeyboardButton(t)).ToList()).ToList());

            /// <summary>Generates a reply keyboard markup with one button</summary>
            /// <param name="button">Keyboard button</param>
            [return: NotNullIfNotNull(nameof(button))]
            public static implicit operator ReplyKeyboardMarkup?(KeyboardButton? button) => button is null ? default : new(button);

            /// <summary>Generates a reply keyboard markup with multiple buttons on one row</summary>
            /// <param name="buttons">Keyboard buttons</param>
            [return: NotNullIfNotNull(nameof(buttons))]
            public static implicit operator ReplyKeyboardMarkup?(KeyboardButton[]? buttons) => buttons is null ? default : new([buttons]);

            /// <summary>Generates a reply keyboard markup with multiple buttons</summary>
            /// <param name="buttons">Keyboard buttons</param>
            [return: NotNullIfNotNull(nameof(buttons))]
            public static implicit operator ReplyKeyboardMarkup?(IEnumerable<KeyboardButton>[]? buttons) => buttons is null ? default : new(buttons);

            /// <summary>Add a button to the last row</summary>
            /// <param name="button">The button or text to add</param>
            public ReplyKeyboardMarkup AddButton(KeyboardButton button)
            {
                if (Keyboard is not List<List<KeyboardButton>> keyboard)
                    throw new InvalidOperationException("This method works only with a List<List<KeyboardButton>> keyboard");
                if (keyboard.Count == 0) keyboard.Add([]);
                keyboard[^1].Add(button);
                return this;
            }

            /// <summary>Add buttons to the last row</summary>
            /// <param name="buttons">The buttons or texts to add</param>
            public ReplyKeyboardMarkup AddButtons(params KeyboardButton[] buttons)
            {
                if (Keyboard is not List<List<KeyboardButton>> keyboard)
                    throw new InvalidOperationException("This method works only with a List<List<KeyboardButton>> keyboard");
                if (keyboard.Count == 0) keyboard.Add([]);
                keyboard[^1].AddRange([.. buttons]);
                return this;
            }

            /// <summary>Add a new row of buttons</summary>
            /// <param name="buttons">Optional: buttons or texts for the new row</param>
            public ReplyKeyboardMarkup AddNewRow(params KeyboardButton[] buttons)
            {
                if (Keyboard is not List<List<KeyboardButton>> keyboard)
                    throw new InvalidOperationException("This method works only with a List<List<KeyboardButton>> keyboard");
                keyboard.Add([.. buttons]);
                return this;
            }
        }

        public partial class InlineKeyboardMarkup
        {
            /// <summary>Initializes a new instance of the <see cref="InlineKeyboardMarkup"/> class with only one keyboard button</summary>
            /// <param name="inlineKeyboardButton">Keyboard button</param>
            [SetsRequiredMembers]
            public InlineKeyboardMarkup(InlineKeyboardButton inlineKeyboardButton) : this(new List<List<InlineKeyboardButton>> { new() { inlineKeyboardButton } }) { }

#pragma warning disable MA0016 // Prefer using collection abstraction instead of implementation
            /// <summary>Initializes a new instance of the <see cref="InlineKeyboardMarkup"/> class with a one-row keyboard</summary>
            /// <param name="inlineKeyboardRow">The inline keyboard row</param>
            [SetsRequiredMembers]
            public InlineKeyboardMarkup(List<InlineKeyboardButton> inlineKeyboardRow) : this(new List<List<InlineKeyboardButton>> { inlineKeyboardRow }) { }
#pragma warning restore MA0016 // Prefer using collection abstraction instead of implementation

            /// <summary>Initializes a new instance of the <see cref="InlineKeyboardMarkup"/> class with a one-row keyboard</summary>
            /// <param name="inlineKeyboardRow">The inline keyboard row</param>
            [SetsRequiredMembers]
            public InlineKeyboardMarkup(IEnumerable<InlineKeyboardButton> inlineKeyboardRow) : this(new List<List<InlineKeyboardButton>> { inlineKeyboardRow.ToList() }) { }

            /// <summary>Initializes a new instance of the <see cref="InlineKeyboardMarkup"/> class with a one-row keyboard</summary>
            /// <param name="inlineKeyboardRow">The inline keyboard row</param>
            [SetsRequiredMembers]
            public InlineKeyboardMarkup(params InlineKeyboardButton[] inlineKeyboardRow) : this(new List<List<InlineKeyboardButton>> { inlineKeyboardRow.ToList() }) { }

            /// <summary>Generate an empty inline keyboard markup</summary>
            /// <returns>Empty inline keyboard markup</returns>
            public static InlineKeyboardMarkup Empty() => new();

            /// <summary>Generate an inline keyboard markup with one button</summary>
            /// <param name="button">Inline keyboard button</param>
            [return: NotNullIfNotNull(nameof(button))]
            public static implicit operator InlineKeyboardMarkup?(InlineKeyboardButton? button) => button is null ? default : new(button);

            /// <summary>Generate an inline keyboard markup with one button</summary>
            /// <param name="buttonText">Text of the button</param>
            [return: NotNullIfNotNull(nameof(buttonText))]
            public static implicit operator InlineKeyboardMarkup?(string? buttonText) => buttonText is null ? default : new(buttonText);

            /// <summary>Generate an inline keyboard markup from multiple buttons on 1 row</summary>
            /// <param name="buttons">Keyboard buttons</param>
            [return: NotNullIfNotNull(nameof(buttons))]
            public static implicit operator InlineKeyboardMarkup?(InlineKeyboardButton[]? buttons) => buttons is null ? default : new(buttons);

            /// <summary>Generate an inline keyboard markup from multiple buttons</summary>
            /// <param name="buttons">Keyboard buttons</param>
            [return: NotNullIfNotNull(nameof(buttons))]
            public static implicit operator InlineKeyboardMarkup?(IEnumerable<InlineKeyboardButton>[]? buttons) => buttons is null ? default : new(buttons);

            /// <summary>Add a button to the last row</summary>
            /// <param name="button">The button to add</param>
            public InlineKeyboardMarkup AddButton(InlineKeyboardButton button)
            {
                if (InlineKeyboard is not List<List<InlineKeyboardButton>> keyboard)
                    throw new InvalidOperationException("This method works only with a List<List<InlineKeyboardButton>> keyboard");
                if (keyboard.Count == 0) keyboard.Add([]);
                keyboard[^1].Add(button);
                return this;
            }

            /// <summary>Add a callback button to the last row</summary>
            /// <param name="text">Label text on the button</param>
            /// <param name="callbackData">Data to be sent in a <see cref="CallbackQuery">callback query</see> to the bot when the button is pressed, 1-64 bytes</param>
            public InlineKeyboardMarkup AddButton(string text, string callbackData)
                => AddButton(InlineKeyboardButton.WithCallbackData(text, callbackData));

            /// <summary>Add buttons to the last row</summary>
            /// <param name="buttons">The buttons to add</param>
            public InlineKeyboardMarkup AddButtons(params InlineKeyboardButton[] buttons)
            {
                if (InlineKeyboard is not List<List<InlineKeyboardButton>> keyboard)
                    throw new InvalidOperationException("This method works only with a List<List<KeyboardButton>> keyboard");
                if (keyboard.Count == 0) keyboard.Add([]);
                keyboard[^1].AddRange([.. buttons]);
                return this;
            }

            /// <summary>Add a new row of buttons</summary>
            /// <param name="buttons">Optional: buttons for the new row</param>
            public InlineKeyboardMarkup AddNewRow(params InlineKeyboardButton[] buttons)
            {
                if (InlineKeyboard is not List<List<InlineKeyboardButton>> keyboard)
                    throw new InvalidOperationException("This method works only with a List<List<InlineKeyboardButton>> keyboard");
                keyboard.Add([.. buttons]);
                return this;
            }
        }

        public partial class InlineKeyboardButton
        {
            /// <summary>Creates an inline keyboard button for external URL or for data to be sent in a <see cref="CallbackQuery">callback query</see> to the bot when the button is pressed, 1-64 bytes</summary>
            /// <param name="text">Label text on the button</param>
            /// <param name="callbackDataOrUrl">URL (starting with http:// or https://) to be opened, or data (1-64 characters) to be sent in a <see cref="CallbackQuery">callback query</see> to the bot, when the button is pressed</param>
            [SetsRequiredMembers]
            public InlineKeyboardButton(string text, string callbackDataOrUrl)
            {
                Text = text;
                if (callbackDataOrUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || callbackDataOrUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    Url = callbackDataOrUrl;
                else
                    CallbackData = callbackDataOrUrl;
            }

            /// <summary>Performs an implicit conversion from <see cref="string"/> to <see cref="InlineKeyboardButton"/></summary>
            /// <param name="textAndCallbackDataOrUrl">Text serving as the label of the button, as well as the URL to be opened or the callback data to be sent</param>
            /// <returns>The result of the conversion.</returns>
            public static implicit operator InlineKeyboardButton(string textAndCallbackDataOrUrl)
                => new(textAndCallbackDataOrUrl, textAndCallbackDataOrUrl);

            /// <summary>Performs an implicit conversion from (<see cref="string"/>, <see cref="string"/>) tuple to <see cref="InlineKeyboardButton"/></summary>
            /// <param name="tuple">Tuple with label text, and the URL to be opened or the callback data</param>
            /// <returns>The result of the conversion.</returns>
            public static implicit operator InlineKeyboardButton((string text, string callbackDataOrUrl) tuple)
                => new(tuple.text, tuple.callbackDataOrUrl);

            /// <summary>Creates an inline keyboard button that sends <see cref="CallbackQuery"/> to bot when pressed</summary>
            /// <param name="textAndCallbackData">Text and data of the button to be sent in a <see cref="CallbackQuery">callback query</see> to the bot when button is pressed, 1-64 bytes</param>
            public static InlineKeyboardButton WithCallbackData(string textAndCallbackData)
                => new(textAndCallbackData) { CallbackData = textAndCallbackData };
        }

        public partial class KeyboardButton
        {
            /// <summary>Generate a keyboard button from text</summary>
            /// <param name="text">Button's text</param>
            public static implicit operator KeyboardButton(string text)
                => new(text);

            /// <summary>Generate a keyboard button to request users</summary>
            /// <param name="text">Button's text</param>
            /// <param name="requestId">Signed 32-bit identifier of the request that will be received back in the <see cref="UsersShared"/> object. Must be unique within the message</param>
            /// <param name="maxQuantity"><em>Optional</em>. The maximum number of users to be selected; 1-10. Defaults to 1.</param>
            public static KeyboardButton WithRequestUsers(string text, int requestId, int? maxQuantity = null)
                => new(text) { RequestUsers = new(requestId) { MaxQuantity = maxQuantity } };

            /// <summary>Creates a keyboard button. Pressing the button will open a list of suitable chats. Tapping on a chat will send its identifier to the bot in a <see cref="ChatShared"/> service message. Available in private chats only.</summary>
            /// <param name="text">Button's text</param>
            /// <param name="requestId">Signed 32-bit identifier of the request, which will be received back in the <see cref="ChatShared"/> object. Must be unique within the message</param>
            /// <param name="chatIsChannel">Pass <see langword="true"/> to request a channel chat, pass <see langword="false"/> to request a group or a supergroup chat.</param>
            public static KeyboardButton WithRequestChat(string text, int requestId, bool chatIsChannel)
                => new(text) { RequestChat = new(requestId, chatIsChannel) };
        }
    }

}
