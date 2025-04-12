using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Settings;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetBotTelegram.Handler;

public class MessageHandler(
    ILogger<MessageHandler> logger,
    ICommandHandler commandHandler,
    ITextMessageHandler textMessageHandler) : IMessageHandler
{
    public async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
    {
        // --- Get Core Information ---
        long chatId = message.Chat.Id;
        long? fromUserId = message.From?.Id; // Use nullable long for safety
        long botId = BotSettings.BotId;

        if (message.Text is not { } messageText)
        {
            logger.LogWarning("Message invalid or empty from chat {ChatId}.", chatId);
            return;
        }

        if (message.Chat.Type != ChatType.Private)
        {
            logger.LogWarning("Received message from a {ChatType} in chat {ChatId}.", message.Chat.Type.ToString(), chatId);
            return;
        }

        if (fromUserId == null)
        {
            logger.LogWarning("Received message in Private Chat (ID: {ChatId}) but sender information is missing.",
                chatId);
            return;
        }
        if (fromUserId == botId)
        {
            // Message from the bot to the user (ChatId)
            logger.LogWarning("Bot (ID: {BotId}) sent a message in a Private Chat with User (ID: {ChatId}).", botId,
                chatId);
            return;
        }
        // Message from the user (FromUserId) to the bot -> fromUserId == chatId

        logger.LogInformation("Received '{MessageText}' from {Username} message in chat {ChatId}.", messageText, message.From?.Username, chatId);

        // Check for commands (entities) and the first entity is a BotCommand
        if (message.Entities is { Length: > 0 } && message.Entities[0].Type == MessageEntityType.BotCommand)
            await commandHandler.HandleCommandAsync(message, cancellationToken);
        else
            await textMessageHandler.HandleTextMessageAsync(message, cancellationToken);
    }
}