using Telegram.Bot;
using Message = SharedLibrary.Telegram.Message;

namespace BudgetAutomation.Engine.Service;

public interface ISenderGateway
{
    Task<Message> ReplyAsync(
        SharedLibrary.Telegram.Chat chat,
        string text,
        string logMessage = "",
        LogLevel logLevel = LogLevel.Information,
        string ParseModeparseMode = default,
        string ReplyParametersQreplyParameters = default,
        string ReplyMarkupQreplyMarkup = default,
        string LinkPreviewOptionsQlinkPreviewOptions = default,
        int? messageThreadId = default,
        IEnumerable<SharedLibrary.Telegram.MessageEntity>? entities = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default);
}

public class SenderGateway(ITelegramBotClient botClient, ILogger<SenderGateway> logger) : ISenderGateway
{
    public async Task<Message> ReplyAsync(
        SharedLibrary.Telegram.Chat chat,
        string text,
        string logMessage = "",
        LogLevel logLevel = LogLevel.Information,
        string ParseModeparseMode = default,
        string ReplyParametersQreplyParameters = default,
        string ReplyMarkupQreplyMarkup = default,
        string LinkPreviewOptionsQlinkPreviewOptions = default,
        int? messageThreadId = default,
        IEnumerable<SharedLibrary.Telegram.MessageEntity>? entities = default,
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
            // parseMode: parseMode, // TODO: fix these parameters on the new reply service
            // replyParameters: replyParameters,
            // replyMarkup: replyMarkup,
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