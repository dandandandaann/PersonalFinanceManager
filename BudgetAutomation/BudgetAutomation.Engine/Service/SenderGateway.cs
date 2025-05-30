using BudgetAutomation.Engine.Mapper;
using Telegram.Bot;
using SharedLibrary.Telegram;
using SharedLibrary.Telegram.Enums;
using SharedLibrary.Telegram.Types.ReplyMarkups;

namespace BudgetAutomation.Engine.Service;

public interface ISenderGateway
{
    Task<Message> ReplyAsync(
        Chat chat,
        string text,
        string logMessage = "",
        LogLevel logLevel = LogLevel.Information,
        ParseMode parseMode = default,
        string ReplyParametersQreplyParameters = default,
        ReplyMarkup? replyMarkup = default,
        string LinkPreviewOptionsQlinkPreviewOptions = default,
        int? messageThreadId = default,
        IEnumerable<MessageEntity>? entities = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default);
}

public class SenderGateway(ITelegramBotClient botClient, MessageMapper messageMapper, ILogger<SenderGateway> logger) : ISenderGateway
{
    public async Task<Message> ReplyAsync(
        Chat chat,
        string text,
        string logMessage = "",
        LogLevel logLevel = LogLevel.Information,
        ParseMode parseMode = default,
        string ReplyParametersQreplyParameters = default,
        ReplyMarkup? replyMarkup = default,
        string LinkPreviewOptionsQlinkPreviewOptions = default,
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
            replyMarkup: replyMarkup == null ? null : messageMapper.MapReplyMarkup(replyMarkup),
            // linkPreviewOptions: linkPreviewOptions,
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