using SharedLibrary.Telegram;

namespace TelegramListener;

/// <summary>
/// This class and method were created as a workaround for the fact that I was not able to serialize the Update object
/// on my aot service to send it as a SQS message and deserialize it in the message consumer
/// </summary>
public class TelegramUpdateConverter
{
    public static SharedLibrary.Telegram.Update ConvertUpdate(Telegram.Bot.Types.Update telegramUpdate)
    {
        MessageEntity[]? convertedMessageEntities = null;

        if (telegramUpdate.Message?.Entities?.Length > 0)
        {
            convertedMessageEntities = new SharedLibrary.Telegram.MessageEntity[telegramUpdate.Message.Entities.Length];
            int i = 0;

            foreach (var e in telegramUpdate.Message.Entities)
            {
                var intType = (int)e.Type;
                if (Enum.IsDefined(typeof(Telegram.Bot.Types.Enums.MessageEntityType), intType))
                {
                    convertedMessageEntities[i++] = new MessageEntity
                    {
                        Type = (MessageEntityType)intType,
                        Offset = e.Offset,
                        Length = e.Length,
                        Url = e.Url,
                        User = new User
                        {
                            Id = e.User?.Id ?? 0,
                            Username = e.User?.Username
                        },
                    };
                }
            }
        }

        var simplifiedUpdate = new SharedLibrary.Telegram.Update
        {
            Id = telegramUpdate.Id,
            Message = new Message
            {
                Id = telegramUpdate.Message?.Id ?? 0,
                From = new User
                {
                    Id = telegramUpdate.Message?.From?.Id ?? 0,
                    Username = telegramUpdate.Message?.From?.Username
                },
                Date = telegramUpdate.Message.Date,
                Chat = new Chat
                {
                    Id = telegramUpdate.Message.Chat.Id,
                    Type = (ChatType)(int)telegramUpdate.Message.Chat.Type,
                    Username = telegramUpdate.Message.Chat.Username,
                },
                Text = telegramUpdate.Message.Text,
                Entities = convertedMessageEntities,
                Type = MessageType.Unknown
            },
            Type = UpdateType.Unknown
        };

        return simplifiedUpdate;
    }
}