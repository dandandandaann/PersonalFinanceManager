using BudgetBotTelegram.Interface;
using SharedLibrary.Settings;
using SharedLibrary.Telegram;

namespace BudgetBotTelegram.Handler;

public class MessageHandler(
    ILogger<MessageHandler> logger,
    ICommandHandler commandHandler,
    IUserManagerService userManagerService,
    ITextMessageHandler textMessageHandler) : IMessageHandler
{
    public async Task HandleMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        // --- Get Core Information ---
        long chatId = message.Chat.Id;

        // long.TryParse(message.From?.Id, out long fromUserId);

        long fromUserId = message.From?.Id ?? 0; // Use nullable long for safety
        long botId = BotSettings.Id;

        if (message.Text is not { } messageText)
        {
            logger.LogWarning("Message invalid or empty from chat {ChatId}.", chatId);
            return;
        }

        if (message.Chat.Type != ChatType.Private)
        {
            logger.LogWarning("Received message from a Chat Type {ChatType} in chat {ChatId}.", message.Chat.Type.ToString(), chatId);
            return;
        }

        if (fromUserId == 0)
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

        if (!userManagerService.AuthenticateUser(fromUserId, cancellationToken))
            logger.LogInformation("Received '{MessageText}' from new user {Username} in chat {ChatId}.", messageText,
                message.From?.Username, chatId);
        else
            logger.LogInformation("Received '{MessageText}' from registered user {Username} in chat {ChatId}.", messageText,
                message.From?.Username, chatId);

        // Check for commands (entities) and the first entity is a BotCommand
        if (message.Entities is { Length: > 0 } && message.Entities[0].Type == MessageEntityType.BotCommand)
            await commandHandler.HandleCommandAsync(message, cancellationToken);
        else
            await textMessageHandler.HandleTextMessageAsync(message, cancellationToken);
    }
}