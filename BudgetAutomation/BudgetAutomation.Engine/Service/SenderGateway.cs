using BudgetAutomation.Engine.Mapper;
using Telegram.Bot;
using SharedLibrary.Telegram.Enums;
using SharedLibrary.Telegram.Types.ReplyMarkups;
using Chat = SharedLibrary.Telegram.Chat;
using Message = SharedLibrary.Telegram.Message;
using MessageEntity = SharedLibrary.Telegram.MessageEntity;

namespace BudgetAutomation.Engine.Service;

public interface ISenderGateway
{
    Task<Message> ReplyAsync(
        Chat chat,
        string text,
        string logMessage = "",
        LogLevel logLevel = LogLevel.Information,
        ParseMode parseMode = default,
        // ReplyParameters? replyParameters = default,
        ReplyMarkup? replyMarkup = default,
        // LinkPreviewOptions? linkPreviewOptions = default,
        int? messageThreadId = default,
        IEnumerable<MessageEntity>? entities = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default);
}

public class SenderGateway(ITelegramBotClient botClient, ReplyMarkupMapper replyMarkupMapper, ILogger<SenderGateway> logger) : ISenderGateway
{
    public async Task<Message> ReplyAsync(
        Chat chat,
        string text,
        string logMessage = "",
        LogLevel logLevel = LogLevel.Information,
        ParseMode parseMode = default,
        // ReplyParameters? replyParameters = default,
        ReplyMarkup? replyMarkup = default,
        // LinkPreviewOptions? linkPreviewOptions = default,
        int? messageThreadId = default,
        IEnumerable<MessageEntity>? entities = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default)
    {
        var sentMessage = await botClient.SendMessage(
            chatId: chat.Id,
            text: text,
            parseMode: System.Enum.Parse<Telegram.Bot.Types.Enums.ParseMode>(parseMode.ToString()),
            // replyParameters: replyParameters,
            replyMarkup: replyMarkup == null ? null : replyMarkupMapper.Map(replyMarkup),
            // linkPreviewOptions: new LinkPreviewOptions {IsDisabled = true},
            messageThreadId: messageThreadId,
            // entities: entities,
            disableNotification: disableNotification,
            protectContent: protectContent,
            messageEffectId: messageEffectId,
            businessConnectionId: businessConnectionId,
            allowPaidBroadcast: allowPaidBroadcast,
            cancellationToken: cancellationToken
        );

        const string logDefault = "ChatId: {ChatId}. Username: {Username}.";
        logMessage = string.IsNullOrWhiteSpace(logMessage) ? $"Sent message. {logDefault}" : $"Sent. {logMessage} {logDefault}";
        logger.Log(logLevel, logMessage, chat.Id, chat.Username);

        return new Message();// sentMessage;
    }
}