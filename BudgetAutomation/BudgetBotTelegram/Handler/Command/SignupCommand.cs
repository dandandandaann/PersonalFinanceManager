using System.Text;
using System.Text.RegularExpressions;
using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Model;
using BudgetBotTelegram.Service;
using SharedLibrary.Telegram;

namespace BudgetBotTelegram.Handler.Command;

public partial class SignupCommand(
    ISenderGateway sender,
    IUserApiClient userApiClient) : ISignupCommand
{
    public const string CommandName = "signup";

    public async Task<Message> HandleSignupAsync(Message message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.From, nameof(message.From));
        ArgumentNullException.ThrowIfNull(message.Text);

        var telegramId = message.From.Id;
        var username = message.From.Username;

        // Send an initial reply indicating the process has started
        _ = sender.ReplyAsync(
            message.Chat,
            "Attempting to sign you up...", "Signup process started.",
            cancellationToken: cancellationToken);

        try
        {
            if (!TryParseCommandArguments(message.Text, CommandName, out string signupArguments))
            {
                throw new InvalidUserInputException($"Message text doesn't start with {CommandName} command.");
            }

            if (string.IsNullOrWhiteSpace(signupArguments) ||
                !EmailRegex().IsMatch(signupArguments))
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

    // TODO: unify this method with LogCommand's
    private static bool TryParseCommandArguments(string text, string commandName, out string arguments)
    {
        arguments = "";
        string commandWithSlash = "/" + commandName;
        int prefixLength = -1;

        // Check if it starts with "/command" (case-insensitive)
        if (text.StartsWith(commandWithSlash, StringComparison.OrdinalIgnoreCase))
        {
            prefixLength = commandWithSlash.Length;
        }
        else
        {
            // Doesn't start with the command at all
            return false;
        }

        // Now check what follows the command prefix
        // The message is exactly the command (e.g., "/log" or "log")
        if (text.Length == prefixLength)
        {
            arguments = string.Empty;
            return true;
        }

        // The command is followed by a space (e.g., "/log args" or "log args")
        if (text.Length > prefixLength && text[prefixLength] == ' ')
        {
            // Extract arguments, trim potential whitespace around them
            arguments = text.Substring(prefixLength + 1).Trim();
            return true;
        }

        // It's something else (e.g., "/logfoobar" or "logfoobar") - invalid command invocation
        return false;
    }

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
    private static partial Regex EmailRegex();
}