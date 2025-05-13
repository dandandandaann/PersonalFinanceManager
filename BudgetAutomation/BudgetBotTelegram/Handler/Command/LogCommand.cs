using System.Text.RegularExpressions;
using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Model;
using BudgetBotTelegram.Service;
using SharedLibrary;
using SharedLibrary.Telegram;

namespace BudgetBotTelegram.Handler.Command;

public partial class LogCommand(
    ISenderGateway sender,
    IExpenseLoggerApiClient expenseApiClient,
    IChatStateService chatStateService) : ILogCommand
{
    public const string CommandName = "log";

    public async Task<Message> HandleLogAsync(Message message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Text);

        // TODO: is it possible to make a virtual class with this verification? Is it worth it?
        if (!UserManagerService.UserLoggedIn)
            throw new UnauthorizedAccessException();

        if (!TryParseCommandArguments(message.Text, CommandName, out string expenseArguments))
        {
            throw new InvalidUserInputException($"Message text doesn't start with {CommandName} command.");
        }

        if (String.IsNullOrEmpty(expenseArguments))
        {
            var chatState = ChatStateService.StateEnum.AwaitingLogArguments.ToString();
            await chatStateService.SetStateAsync(message.Chat.Id, chatState); // TODO: create enum for chat states

            return await sender.ReplyAsync(message.Chat,
                "Okay, please enter the details for your expense. e.g. 'Café 5,50 Comida'",
                $"Chat state: {chatState}.",
                cancellationToken: cancellationToken);
        }

        return await LogExpenseAsync(message.Chat, expenseArguments, cancellationToken);
    }

    public async Task<Message> HandleLogAsync(Message message, ChatState chatState, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(message.Text);

        await chatStateService.ClearState(message.Chat.Id);

        if (chatState.State == ChatStateService.StateEnum.AwaitingLogArguments.ToString())
        {
            return await LogExpenseAsync(message.Chat, message.Text, cancellationToken);
        }

        return await sender.ReplyAsync(message.Chat, $"Log state {chatState.State} not implemented.",
            $"Log state {chatState} not implemented.",
            logLevel: LogLevel.Error,
            cancellationToken: cancellationToken
        );
    }

    private async Task<Message> LogExpenseAsync(Chat chatId, string expenseArguments,
        CancellationToken cancellationToken = default)
    {
        var expense = ParseExpenseArguments(expenseArguments);

        try
        {
            expense = await expenseApiClient.LogExpenseAsync(expense, cancellationToken);

            return await sender.ReplyAsync(chatId,
                $"Logged Expense\n{expense}",
                "Logged expense.",
                cancellationToken: cancellationToken);
        }
        catch (ArgumentException e)
        {
            return await sender.ReplyAsync(chatId,
                "Failed to log expense.", e.Message, logLevel: LogLevel.Error,
                cancellationToken: cancellationToken);
        }
    }

    private static Expense ParseExpenseArguments(string expenseArguments)
    {
        var expense = new Expense();
        var match = ExpenseArgumentsRegex().Match(expenseArguments);

        if (match.Success)
        {
            expense.Description = match.Groups[1].Value;
            expense.Amount = match.Groups[2].Value;
            expense.Category = match.Groups[3].Value;
        }
        else // try to split by space
        {
            var argumentsSplit = expenseArguments.Trim().Split(' ');

            if (argumentsSplit.Length < 2)
                throw new InvalidUserInputException("Invalid message format for logging expense.");

            expense.Description = argumentsSplit[0];
            expense.Amount = argumentsSplit[1];
            expense.Category = argumentsSplit.Length >= 3 ? argumentsSplit[2] : string.Empty;
        }

        if (!decimal.TryParse(expense.Amount, out _))
        {
            throw new InvalidUserInputException("Invalid amount format for logging expense.");
        }

        return expense;
    }

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
        // Check if it starts with "command" (case-insensitive)
        else if (text.StartsWith(commandName, StringComparison.OrdinalIgnoreCase))
        {
            prefixLength = commandName.Length;
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

    [GeneratedRegex(@"^(.+)\s+([\d.,]+)\s*(.*)$")]
    private static partial Regex ExpenseArgumentsRegex();
}