using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetBotTelegram.Handler;

public class MessageHandler(ILogger<MessageHandler> logger, CommandHandler commandHandler, TextMessageHandler textMessageHandler)
{

    public async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received message type: {MessageType}", message.Type);
        if (message.Text is not { } messageText) return;

        logger.LogInformation("Received '{MessageText}' message in chat {ChatId}.", messageText, message.Chat.Id);

        // Check for commands (entities) and the first entity is a BotCommand
        if (message.Entities is { Length: > 0 } && message.Entities[0].Type == MessageEntityType.BotCommand)
            await commandHandler.HandleCommandAsync(message, cancellationToken);
        else
            await textMessageHandler.HandleTextMessageAsync(message, cancellationToken);
    }
}