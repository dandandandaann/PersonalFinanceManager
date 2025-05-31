using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Misc;
using BudgetAutomation.Engine.Service;
using SharedLibrary.Telegram;
using SharedLibrary.Telegram.Enums;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace BudgetAutomation.Engine.Handler;

public class UpdateHandler(
    ISenderGateway sender,
    IMessageHandler messageHandler,
    ITelegramBotClient botClient,
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
        catch (UnauthorizedAccessException ex)
        {
            var message = update.Message ?? update.CallbackQuery!.Message;

            await sender.ReplyAsync(message!.Chat, "Por favor, faça o cadastro para continuar.",
                $"UnauthorizedAccessException: {ex.Message} User message: {message.Text}.",
                logLevel: LogLevel.Warning,
                cancellationToken: cancellationToken);
        }
        catch (InvalidUserInputException ex)
        {
            var message = update.Message ?? update.CallbackQuery!.Message;

            var replyMessage = string.IsNullOrWhiteSpace(ex.Message)
                ? "Sua mensagem estava inválida de alguma forma. Por favor, tente algo diferente."
                : ex.Message;

            await sender.ReplyAsync(message!.Chat, replyMessage,
                $"InvalidUserInputException: {ex.Message}. User message: {message.Text}.",
                logLevel: LogLevel.Information,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError("Error handling update. Exception: {Exception}", ex);
            throw;
        }
    }

    private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Received callback query with data: {CallbackData}", callbackQuery.Data);

        try
        {
            await botClient.AnswerCallbackQuery(
                callbackQueryId: callbackQuery.Id,
                cancellationToken: cancellationToken);
        }
        catch (ApiRequestException ex) when (ex.Message.Contains("query is too old"))
        {
        }

        var simulatedMessage = new Message
        {
            Id = callbackQuery.Message!.Id,
            From = callbackQuery.From,
            Chat = callbackQuery.Message.Chat,
            Date = DateTime.UtcNow,
            Text = callbackQuery.Data,
            Type = MessageType.Text
        };

        await messageHandler.HandleMessageAsync(simulatedMessage, cancellationToken);
    }

    private async Task HandleUnknownUpdateAsync(Update update, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Unknown update type: {UpdateType}", update.Type);

        if (update.Message != null)
        {
            await sender.ReplyAsync(
                chat: update.Message.Chat,
                text: $"I can't handle message type {update.Type}.",
                cancellationToken: cancellationToken);
        }
    }

    private void HandlePollingErrorAsync(Exception ex)
    {
    }
}