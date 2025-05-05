using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Model;
using BudgetBotTelegram.Service;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetBotTelegram.Handler;

public class UpdateHandler(
    ITelegramBotClient botClient,
    ISenderGateway sender,
    IMessageHandler messageHandler,
    ILogger<UpdateHandler> logger) : IUpdateHandler
{

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken = default)
    {
        var handler = update.Type switch
        {
            // Handle different update types
            UpdateType.Message => messageHandler.HandleMessageAsync(update.Message!, cancellationToken),
            UpdateType.CallbackQuery => HandleCallbackQueryAsync(update.CallbackQuery!, cancellationToken),
            // Add handlers for other update types as needed (EditedMessage, ChannelPost, etc.)
            _ => HandleUnknownUpdateAsync(update, cancellationToken)
        };

        try
        {
            await handler;
        }
        catch (UnauthorizedAccessException e)
        {
            var message = update.Message ?? update.CallbackQuery!.Message;

            await sender.ReplyAsync(message!.Chat, "Please signup to proceed.",
                $"UnauthorizedAccessException: {e.Message}. User message: {message.Text}.",
                logLevel: LogLevel.Warning,
                cancellationToken: cancellationToken);

        }
        catch (InvalidUserInputException e)
        {
            var message = update.Message ?? update.CallbackQuery!.Message;

            await sender.ReplyAsync(message!.Chat, "Your message was invalid somehow. Please try something else.",
                $"InvalidUserInputException: {e.Message}. User message: {message.Text}.",
                logLevel: LogLevel.Information,
                cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            HandlePollingErrorAsync(exception);
        }
    }

    private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Received callback query with data: {CallbackData}", callbackQuery.Data);

        // Acknowledge the callback query is required
        await botClient.AnswerCallbackQuery( // TODO: implement this method in SenderGateway
            callbackQueryId: callbackQuery.Id,
            text: $"Received {callbackQuery.Data}",
            cancellationToken: cancellationToken);

        // Add logic to handle the callback query data
        // Example: Modify the message or perform an action based on callbackQuery.Data
    }

    private async Task HandleUnknownUpdateAsync(Update update, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Unknown update type: {UpdateType}", update.Type);

        if (update.Message != null)
        {
            await botClient.SendMessage(
                chatId: update.Message.Chat.Id,
                text: $"I can't handle message type {update.Type}.",
                cancellationToken: cancellationToken);
        }
    }

    private void HandlePollingErrorAsync(Exception exception)
    {
        var errorMessage = exception switch
        {
            // Handle specific Telegram API exceptions if needed
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        logger.LogError("Error handling update: {ErrorMessage}", errorMessage);
    }
}