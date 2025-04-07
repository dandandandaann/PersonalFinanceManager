using BudgetBotTelegram.Handler.Command;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetBotTelegram.Handler;

public class MessageHandler(SenderGateway sender, ILogger<MessageHandler> logger, CommandHandler commandHandler, LogCommand log)
{
    public async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received message type: {MessageType}", message.Type);
        if (message.Text is not { } messageText) return;

        var chatId = message.Chat.Id;
        string replyMessage = string.Empty, logMessage = string.Empty;
        logger.LogInformation("Received '{MessageText}' message in chat {ChatId}.", messageText, chatId);

        // Check for commands (entities) and the first entity is a BotCommand
        if (message.Entities is { Length: > 0 } && message.Entities[0].Type == MessageEntityType.BotCommand)
        {
            await commandHandler.HandleCommandAsync(message, cancellationToken);
        }
        else
        {
            switch (messageText) // TODO: work with natural language?
            {
                case { } when messageText.StartsWith("log ", StringComparison.CurrentCultureIgnoreCase):

                    await log.HandleLogAsync(message, cancellationToken);

                    break;

                default:
                    replyMessage = "You said:\n" + messageText;
                    break;
            }
        }

        await sender.ReplyAsync(message.Chat, replyMessage, logMessage, cancellationToken: cancellationToken);
    }
}