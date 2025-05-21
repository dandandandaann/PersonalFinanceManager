using System.Text;
using System.Text.RegularExpressions;
using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Misc;
using BudgetBotTelegram.Model;
using BudgetBotTelegram.Service;
using SharedLibrary.Telegram;

namespace BudgetBotTelegram.Handler.Command;

public partial class SignupCommand(
    ISenderGateway sender,
    IUserApiClient userApiClient) : ICommand
{
    public string CommandName => "signup";

    public async Task<Message> HandleAsync(Message message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.From);
        ArgumentNullException.ThrowIfNull(message.Text);

        var telegramId = message.From.Id;
        var username = message.From.Username;

        // Send an initial reply indicating the process has started
        var replyAttempting = sender.ReplyAsync(
            message.Chat,
            "Attempting to sign you up...", "Signup process started.",
            cancellationToken: cancellationToken);

        if (UserManagerService.UserSignedIn)
        {
            await replyAttempting;
            return await sender.ReplyAsync(message.Chat,
                "Signup failed. You are already signed in.",
                "User signup failed (already signed in).",
                logLevel: LogLevel.Warning,
                cancellationToken: cancellationToken);
        }

        try
        {
            if (
                !Utility.TryExtractCommandArguments(message.Text, CommandName, EmailRegex, out var signupArguments) ||
                string.IsNullOrWhiteSpace(signupArguments)
                )
            {
                return await sender.ReplyAsync(message.Chat,
                    "Please include your email for signing up.",
                    $"User tried signing up with bad arguments: '{signupArguments}'.",
                    logLevel: LogLevel.Information,
                    cancellationToken: cancellationToken);
            }

            var response = await userApiClient.SignupUserAsync(
                telegramId, username: username, email: signupArguments, cancellationToken: cancellationToken);

            if (!response.Success)
            {
                // UserApiClient returns Success=false if user already exists or on API error
                // TODO: Differentiate between 'already exists' and 'other error' in UserApiClient response
                return await sender.ReplyAsync(message.Chat,
                    "Signup failed. You might already be registered.",
                    "User signup failed (already exists or API error).",
                    logLevel: LogLevel.Warning,
                    cancellationToken: cancellationToken);
            }

            var welcomeMessage = new StringBuilder();
            welcomeMessage.AppendLine("Signup successful.");
            welcomeMessage.AppendLine();
            welcomeMessage.AppendLine(response.User?.Username == null ? "**Welcome!**" : $"**Welcome, {response.User.Username}!**");
            welcomeMessage.AppendLine("Please type /start to see all available commands and /setup to configure your spreadsheet.");

            return await sender.ReplyAsync(message.Chat,
                welcomeMessage.ToString(),
                "User signup successful.",
                // parseMode: ParseMode.MarkdownV2,
                cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            if (e is InvalidUserInputException or UnauthorizedAccessException)
                throw;

            // Catch exceptions from the API client (e.g., network issues, deserialization errors)
            return await sender.ReplyAsync(message.Chat,
                "An error occurred during signup. Please try again later.",
                $"Exception during signup: {e.Message}",
                logLevel: LogLevel.Error,
                cancellationToken: cancellationToken);
        }
    }

    public Task<Message> HandleAsync(Message message, ChatState chatState, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
    private static partial Regex EmailRegex();
}