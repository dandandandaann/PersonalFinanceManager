using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Model;
using BudgetBotTelegram.Service;
using Telegram.Bot.Types;

namespace BudgetBotTelegram.Handler.Command;

public class SignupCommand(
    ISenderGateway sender,
    IUserApiClient userApiClient,
    IChatStateService chatStateService) : ISignupCommand
{
    public const string CommandName = "signup";

    public async Task<Message> HandleSignupAsync(Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.From, nameof(message.From)); // Need user info for signup

        var telegramId = message.From.Id;
        var username = message.From.Username;

        // Send an initial reply indicating the process has started
        await sender.ReplyAsync(message.Chat, "Attempting to sign you up...", "Signup process started.", cancellationToken: cancellationToken);

        try
        {
            var response = await userApiClient.SignupUserAsync(telegramId, username, cancellationToken);

            if (response.Success)
            {
                return await sender.ReplyAsync(message.Chat,
                    $"Signup successful! Welcome, {response.User?.Username ?? "user"}!",
                    "User signup successful.",
                    cancellationToken: cancellationToken);
            }
            else
            {
                // UserApiClient returns Success=false if user already exists or on API error
                // TODO: Differentiate between 'already exists' and 'other error' in UserApiClient response
                return await sender.ReplyAsync(message.Chat,
                    "Signup failed. You might already be registered, or an error occurred.",
                    "User signup failed (already exists or API error).",
                    logLevel: LogLevel.Warning,
                    cancellationToken: cancellationToken);
            }
        }
        catch (Exception e)
        {
            // Catch exceptions from the API client (e.g., network issues, deserialization errors)
            return await sender.ReplyAsync(message.Chat,
                "An error occurred during signup. Please try again later.",
                $"Exception during signup: {e.Message}",
                logLevel: LogLevel.Error,
                cancellationToken: cancellationToken);
        }
    }

    public async Task<Message> HandleSignupAsync(Message message, ChatState chatState, CancellationToken cancellationToken)
    {
        // Clear state as this simple signup doesn't use multi-step interaction yet
        await chatStateService.ClearState(message.Chat.Id);

        // Currently, no stateful signup process is defined.
        return await sender.ReplyAsync(message.Chat.Id, 
            $"Signup state '{chatState.State}' not implemented.", 
            $"Signup state {chatState} not implemented.",
            logLevel: LogLevel.Warning, // Use Warning as it's an unimplemented feature, not necessarily an error
            cancellationToken: cancellationToken
        );
    }
} 