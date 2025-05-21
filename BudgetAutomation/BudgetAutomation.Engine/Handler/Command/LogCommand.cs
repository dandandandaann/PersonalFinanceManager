using System.Text.RegularExpressions;
using BudgetAutomation.Engine.Enum;
using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Misc;
using BudgetAutomation.Engine.Model;
using BudgetAutomation.Engine.Service;
using SharedLibrary.Model;
using SharedLibrary.Telegram;

namespace BudgetAutomation.Engine.Handler.Command;

public partial class LogCommand(
    ISenderGateway sender,
    IExpenseLoggerApiClient expenseApiClient,
    IChatStateService chatStateService)
    : ICommand
{
    public string CommandName => "log";

    public async Task<Message> HandleAsync(Message message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Text);

        UserManagerService.EnsureUserSignedIn();

        if (!TryExtractCommandArguments(message.Text, CommandName, out string expenseArguments))
        {
            throw new InvalidUserInputException($"Message text doesn't start with {CommandName} command.");
        }

        if (String.IsNullOrEmpty(expenseArguments))
        {
            await chatStateService.SetStateAsync(message.Chat.Id, ChatStateEnum.AwaitingArguments, CommandName);

            return await sender.ReplyAsync(message.Chat,
                "Okay, please enter the details for your expense. e.g. 'Café 5,50 Comida'",
                $"Chat state: {ChatStateEnum.AwaitingArguments}.",
                cancellationToken: cancellationToken);
        }

        return await LogExpenseAsync(message.Chat, UserManagerService.Configuration.SpreadsheetId, expenseArguments,
            cancellationToken);
    }

    public async Task<Message> HandleAsync(Message message, ChatState chatState, CancellationToken cancellationToken = default)
    {
        UserManagerService.EnsureUserSignedIn();

        if (string.IsNullOrWhiteSpace(UserManagerService.Configuration.SpreadsheetId))
        {
            return await sender.ReplyAsync(message.Chat,
                $"Por favor configure sua planilha com o commando /{SpreadsheetCommand.StaticCommandName} antes de " +
                $"usar o comando /{CommandName}.",
                cancellationToken: cancellationToken);
        }

        ArgumentException.ThrowIfNullOrEmpty(message.Text);

        await chatStateService.ClearState(message.Chat.Id);

        if (chatState.State == ChatStateEnum.AwaitingArguments.ToString())
        {
            return await LogExpenseAsync(message.Chat, UserManagerService.Configuration.SpreadsheetId, message.Text,
                cancellationToken);
        }

        return await sender.ReplyAsync(message.Chat, $"Log state {chatState.State} not implemented.",
            $"Log state {chatState} not implemented.",
            logLevel: LogLevel.Error,
            cancellationToken: cancellationToken
        );
    }

    private async Task<Message> LogExpenseAsync(Chat chat, string spreadsheetId, string expenseArguments,
        CancellationToken cancellationToken = default)
    {
        UserManagerService.EnsureUserSignedIn();

        if (string.IsNullOrWhiteSpace(UserManagerService.Configuration.SpreadsheetId))
        {
            return await sender.ReplyAsync(chat,
                $"Por favor configure sua planilha com o commando /{SpreadsheetCommand.StaticCommandName} antes de " +
                $"usar o comando /{CommandName}.",
                cancellationToken: cancellationToken);
        }

        var expense = MapExpenseArguments(expenseArguments);

        try
        {
            expense = await expenseApiClient.LogExpenseAsync(spreadsheetId, expense, cancellationToken);

            return await sender.ReplyAsync(chat,
                $"Logged Expense\n{expense}",
                "Logged expense.",
                cancellationToken: cancellationToken);
        }
        catch (ArgumentException e)
        {
            return await sender.ReplyAsync(chat,
                "Failed to log expense.", e.Message, logLevel: LogLevel.Error,
                cancellationToken: cancellationToken);
        }
    }

    private static Expense MapExpenseArguments(string expenseArguments)
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

    // TODO: join this method with Utility.TryExtractCommandArguments
    private static bool TryExtractCommandArguments(string text, string commandName, out string arguments)
    {
        arguments = "";
        string commandWithSlash = "/" + commandName;
        int prefixLength;

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

        return false;
    }

    [GeneratedRegex(@"^(.+)\s+([\d.,]+)\s*(.*)$")]
    private static partial Regex ExpenseArgumentsRegex();
}