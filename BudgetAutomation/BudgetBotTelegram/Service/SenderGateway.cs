﻿using SharedLibrary.Telegram;
// using Telegram.Bot;
// using Telegram.Bot.Types;
// using Telegram.Bot.Types.Enums;
// using Telegram.Bot.Types.ReplyMarkups;

namespace BudgetBotTelegram.Service;

public interface ISenderGateway
{
    Task<Message> ReplyAsync(
        Chat chatId,
        string text,
        string logMessage = "",
        LogLevel logLevel = LogLevel.Information,
        string ParseModeparseMode = default,
        string ReplyParametersQreplyParameters = default,
        string ReplyMarkupQreplyMarkup = default,
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

public class SenderGateway(string ITelegramBotClientbotClient, ILogger<SenderGateway> logger) : ISenderGateway
{
    public async Task<Message> ReplyAsync(
        Chat chatId,
        string text,
        string logMessage = "",
        LogLevel logLevel = LogLevel.Information,
        string ParseModeparseMode = default,
        string ReplyParametersQreplyParameters = default,
        string ReplyMarkupQreplyMarkup = default,
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
        // var sentMessage = await botClient.SendMessage(
        //     chatId: chatId,
        //     text: text,
        //     parseMode: parseMode,
        //     replyParameters: replyParameters,
        //     replyMarkup: replyMarkup,
        //     linkPreviewOptions: linkPreviewOptions,
        //     messageThreadId: messageThreadId,
        //     entities: entities,
        //     disableNotification: disableNotification,
        //     protectContent: protectContent,
        //     messageEffectId: messageEffectId,
        //     businessConnectionId: businessConnectionId,
        //     allowPaidBroadcast: allowPaidBroadcast,
        //     cancellationToken: cancellationToken
        // );

        const string logDefault = "ChatId: {ChatId}. Username: {Username}.";
        logMessage = string.IsNullOrWhiteSpace(logMessage) ? $"Sent message. {logDefault}" : $"Sent. {logMessage} {logDefault}";
        logger.Log(logLevel, logMessage, chatId.Username);

        return new Message();//sentMessage;
    }
}