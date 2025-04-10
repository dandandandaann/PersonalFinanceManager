using BudgetBotTelegram.Interface;
using Telegram.Bot.Types;

namespace BudgetBotTelegram.Handler;

public class TextMessageHandler(ISenderGateway sender, ILogger<TextMessageHandler> logger, ILogCommand log) : ITextMessageHandler
{
    public async Task HandleTextMessageAsync(Message message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received message type: {MessageType}", message.Type);
        if (message.Text is not { } messageText) return;

        var chatId = message.Chat.Id;
        string replyMessage = string.Empty, logMessage = string.Empty;
        logger.LogInformation("Received '{MessageText}' message in chat {ChatId}.", messageText, chatId);


        switch (messageText)
        {
            case { } when messageText.StartsWith("log ", StringComparison.CurrentCultureIgnoreCase):

                await log.HandleLogAsync(message, cancellationToken);

                break;

            default: // TODO: work with natural language?
                replyMessage = "You said:\n" + messageText;
                break;
        }

        await sender.ReplyAsync(message.Chat, replyMessage, logMessage, cancellationToken: cancellationToken);
    }
}