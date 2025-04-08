using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BudgetBotTelegram;

public interface ISenderGateway
{
    Task<Message> ReplyAsync(
        ChatId chatId,
        string text,
        string logMessage = "",
        ParseMode parseMode = default,
        ReplyParameters? replyParameters = default,
        ReplyMarkup? replyMarkup = default,
        LinkPreviewOptions? linkPreviewOptions = default,
        int? messageThreadId = default,
        IEnumerable<MessageEntity>? entities = default,
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
        ChatId chatId,
        string text,
        string logMessage = "",
        ParseMode parseMode = default,
        ReplyParameters? replyParameters = default,
        ReplyMarkup? replyMarkup = default,
        LinkPreviewOptions? linkPreviewOptions = default,
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
            chatId: chatId,
            text: text,
            parseMode: parseMode,
            replyParameters: replyParameters,
            replyMarkup: replyMarkup,
            linkPreviewOptions: linkPreviewOptions,
            messageThreadId: messageThreadId,
            entities: entities,
            disableNotification: disableNotification,
            protectContent: protectContent,
            messageEffectId: messageEffectId,
            businessConnectionId: businessConnectionId,
            allowPaidBroadcast: allowPaidBroadcast,
            cancellationToken: cancellationToken
        );

        const string logDefault = "MessageId: {MessageId}. UserId: {UserId}.";
        logMessage = string.IsNullOrWhiteSpace(logMessage) ? logDefault : $"{logMessage} {logDefault}";
        logger.LogInformation(logMessage, sentMessage.MessageId, chatId.Username);

        // Call the underlying SendMessageAsync method from the Telegram.Bot library
        return sentMessage;
    }
}