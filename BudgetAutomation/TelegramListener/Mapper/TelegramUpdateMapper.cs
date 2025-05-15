using SharedLibrary.Telegram;
using SharedLibrary.Telegram.Enums;

namespace TelegramListener.Mapper;

/// <summary>
/// This class and method were created as a workaround since I was not able to serialize the whole Telegram Update object
/// on my aot service to send it as a SQS message and deserialize it in the message consumer
/// </summary>
public static class TelegramUpdateMapper
{
    public static Update ConvertUpdate(Telegram.Bot.Types.Update telegramUpdate)
    {
        MessageEntity[]? convertedMessageEntities = null;

        if (telegramUpdate.Message?.Entities?.Length > 0)
        {
            convertedMessageEntities = new MessageEntity[telegramUpdate.Message.Entities.Length];
            var i = 0;

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
                        }
                    };
                }
            }
        }

        Message? message = null;
        if (telegramUpdate.Message != null)
        {
            message = new Message
                {
                    Id = telegramUpdate.Message?.Id ?? 0,
                    From = new User
                    {
                        Id = telegramUpdate.Message?.From?.Id ?? 0,
                        Username = telegramUpdate.Message?.From?.Username
                    },
                    Date = telegramUpdate.Message!.Date,
                    Chat = new Chat
                    {
                        Id = telegramUpdate.Message.Chat.Id,
                        Type = (ChatType)(int)telegramUpdate.Message.Chat.Type,
                        Username = telegramUpdate.Message.Chat.Username,
                    },
                    Text = telegramUpdate.Message.Text,
                    Entities = convertedMessageEntities,
                    Type = (MessageType)(int)telegramUpdate.Message.Type
                };
        }

        var simplifiedUpdate = new Update
        {
            Id = telegramUpdate.Id,
            Message = message,
            Type = (UpdateType)(int)telegramUpdate.Type
        };

        return simplifiedUpdate;
    }
}