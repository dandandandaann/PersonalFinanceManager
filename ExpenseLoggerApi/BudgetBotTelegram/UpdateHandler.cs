using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetBotTelegram;

public class UpdateHandler(
    ITelegramBotClient botClient,
    ExpenseLoggerApiClient expenseApiClient,
    ILogger<UpdateHandler> logger)
{
    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        var handler = update.Type switch
        {
            // Handle different update types
            UpdateType.Message => HandleMessageAsync(update.Message!, cancellationToken),
            UpdateType.CallbackQuery => HandleCallbackQueryAsync(update.CallbackQuery!, cancellationToken),
            // Add handlers for other update types as needed (EditedMessage, ChannelPost, etc.)
            _ => HandleUnknownUpdateAsync(update, cancellationToken)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandlePollingErrorAsync(exception, cancellationToken);
        }
    }

    private async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received message type: {MessageType}", message.Type);
        if (message.Text is not { } messageText) return;

        var chatId = message.Chat.Id;
        string replyMessage, logMessage;
        logger.LogInformation("Received '{MessageText}' message in chat {ChatId}.", messageText, chatId);

        switch (messageText)
        {
            case { } when messageText.StartsWith("log "):
            case { } when messageText.StartsWith("/log "):

                try
                {
                    var expense = await expenseApiClient.LogExpenseAsync(messageText, cancellationToken);
                    replyMessage = $"Logged Expense - {expense}";
                    logMessage = "Logged expense message sent with Id: {SentMessageId}";
                }
                catch (ArgumentException e)
                {
                    replyMessage = e.Message;
                    logMessage = $"Argument Exception: {e.Message}. MessageId: {{MessageId}}";
                }
                break;

            default:
                replyMessage = "You said:\n" + messageText;
                logMessage = "Echo message sent with Id: {SentMessageId}";
                break;
        }

        var sentMessage = await botClient.SendMessage(
            chatId: chatId,
            text: replyMessage,
            cancellationToken: cancellationToken);
        logger.LogInformation(logMessage, sentMessage.MessageId);
    }

    private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received callback query with data: {CallbackData}", callbackQuery.Data);

        // Acknowledge the callback query is required
        await botClient.AnswerCallbackQuery(
            callbackQueryId: callbackQuery.Id,
            text: $"Received {callbackQuery.Data}",
            cancellationToken: cancellationToken);

        // Add logic to handle the callback query data
        // Example: Modify the message or perform an action based on callbackQuery.Data
    }

    private async Task HandleUnknownUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        logger.LogInformation("Unknown update type: {UpdateType}", update.Type);

        if (update.Message != null)
        {
            var sentMessage = await botClient.SendMessage(
                chatId: update.Message.Chat.Id,
                text: $"I can't handle message type {update.Type}.",
                cancellationToken: cancellationToken);
            logger.LogInformation("Echo message sent with Id: {SentMessageId}", sentMessage.MessageId);
        }
    }

    private Task HandlePollingErrorAsync(Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            // Handle specific Telegram API exceptions if needed
            // ApiRequestException apiRequestException
            //     => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        logger.LogError("Error handling update: {ErrorMessage}", errorMessage);
        return Task.CompletedTask;
    }
}