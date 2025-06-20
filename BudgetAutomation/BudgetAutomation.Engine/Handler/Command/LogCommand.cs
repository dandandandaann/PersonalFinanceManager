using System.Text.RegularExpressions;
using BudgetAutomation.Engine.Enums;
using BudgetAutomation.Engine.Handler.Command.Alias;
using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Misc;
using BudgetAutomation.Engine.Model;
using BudgetAutomation.Engine.Service;
using SharedLibrary.Enum;
using SharedLibrary.Model;
using SharedLibrary.Telegram;

namespace BudgetAutomation.Engine.Handler.Command;

public partial class LogCommand(
    ISenderGateway sender,
    ISpreadsheetManagerApiClient expenseApiClient,
    IChatStateService chatStateService)
    : ICommand
{
    public string CommandName => StaticCommandName;
    public static string StaticCommandName => "log";

    public async Task<Message> HandleAsync(Message message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Text);

        UserManagerService.EnsureUserSignedIn();

        if (string.IsNullOrWhiteSpace(UserManagerService.Configuration.SpreadsheetId))
        {
            return await sender.ReplyAsync(message.Chat,
                $"Por favor configure sua planilha com o commando /{PlanilhaCommandAlias.StaticCommandName} antes de " +
                $"usar o comando /{CommandName}.",
                cancellationToken: cancellationToken);
        }

        if (!TryExtractCommandArguments(message.Text, CommandName, out string expenseArguments))
        {
            throw new InvalidUserInputException($"Message text doesn't start with {CommandName} command.");
        }

        if (String.IsNullOrEmpty(expenseArguments))
        {
            await chatStateService.SetStateAsync(message.Chat.Id, ChatStateEnum.AwaitingArguments, CommandName);

            return await sender.ReplyAsync(message.Chat,
                "Insira os detalhes da sua despesa. Exemplo: 'Almoço 15,90 Restaurante'",
                $"Chat state: {ChatStateEnum.AwaitingArguments}.",
                cancellationToken: cancellationToken);
        }

        return await LogExpenseAsync(message.Chat, UserManagerService.Configuration.SpreadsheetId, expenseArguments,
            cancellationToken);
    }

    public async Task<Message> HandleAsync(Message message, ChatState chatState, CancellationToken cancellationToken = default)
    {
        UserManagerService.EnsureUserSignedIn();

        ArgumentException.ThrowIfNullOrEmpty(message.Text);

        await chatStateService.ClearState(message.Chat.Id);

        if (chatState.State == ChatStateEnum.AwaitingArguments.ToString())
        {
            return await LogExpenseAsync(message.Chat, UserManagerService.Configuration.SpreadsheetId, message.Text,
                cancellationToken);
        }

        throw new NotImplementedException($"Log state {chatState} not implemented.");
    }

    private async Task<Message> LogExpenseAsync(Chat chat, string spreadsheetId, string expenseArguments,
        CancellationToken cancellationToken = default)
    {
        UserManagerService.EnsureUserSignedIn();

        if (string.IsNullOrWhiteSpace(UserManagerService.Configuration.SpreadsheetId))
        {
            return await sender.ReplyAsync(chat,
                $"Por favor configure sua planilha com o commando /{SpreadsheetCommand.StaticCommandName} antes de registrar despesas.",
                cancellationToken: cancellationToken);
        }

        var expense = MapExpenseArguments(expenseArguments);

        try
        {
            var response = await expenseApiClient.LogExpenseAsync(spreadsheetId, expense, cancellationToken);

            if (!response.Success)
            {
                switch (response.ErrorCode)
                {
                    case ErrorCodeEnum.ResourceNotFound:
                        return await sender.ReplyAsync(chat,
                            "O registro falhou porque a planilha ou uma das abas necessárias não existem." +
                            "Tente configurar a planilha novamente.",
                            "User log failed spreadsheet or sheet was not found.",
                            logLevel: LogLevel.Warning,
                            cancellationToken: cancellationToken);
                    case ErrorCodeEnum.UnauthorizedAccess:
                        return await sender.ReplyAsync(chat,
                            "O registro falhou porque o sistema não tem permissão na planilha. " +
                            "Verifique se a planilha está compartilhada corretamente.",
                            "User log failed due to unauthorized access.",
                            logLevel: LogLevel.Warning,
                            cancellationToken: cancellationToken);
                    default:
                        return await sender.ReplyAsync(chat,
                            "O registro falhou. Tente novamente.",
                            "User log failed due to API error.",
                            logLevel: LogLevel.Warning,
                            cancellationToken: cancellationToken);
                }
            }

            return await sender.ReplyAsync(chat,
                $"Despesa registrada\n{response.expense}",
                "Logged expense.",
                cancellationToken: cancellationToken);
        }
        catch (ArgumentException e)
        {
            return await sender.ReplyAsync(chat,
                "Falha ao registrar a despesa.", e.Message, logLevel: LogLevel.Error,
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
                throw new InvalidUserInputException("Formato de mensagem inválido para registrar a despesa.");

            expense.Description = argumentsSplit[0];
            expense.Amount = argumentsSplit[1];
            expense.Category = argumentsSplit.Length >= 3 ? argumentsSplit[2] : string.Empty;
        }

        if (!decimal.TryParse(expense.Amount, out _))
        {
            throw new InvalidUserInputException("Formato de mensagem inválido para registrar a despesa.");
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