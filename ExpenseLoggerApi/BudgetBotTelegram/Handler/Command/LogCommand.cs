using System.Text.RegularExpressions;
using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Model;
using SharedLibrary;
using Telegram.Bot.Types;

namespace BudgetBotTelegram.Handler.Command;

public class LogCommand(
    ISenderGateway sender,
    IExpenseLoggerApiClient expenseApiClient,
    IChatStateService chatStateService,
    ILogger<LogCommand> logger) : ILogCommand
{
    public const string CommandName = "log";

    public async Task<Message> HandleLogAsync(Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Text);

        string expenseArguments;
        // TODO: there's probably a better way to do this
        if (message.Text.StartsWith("/log ", StringComparison.OrdinalIgnoreCase))
            expenseArguments = message.Text[5..];
        else if (message.Text.StartsWith("log ", StringComparison.OrdinalIgnoreCase) ||
                 message.Text.StartsWith("/log", StringComparison.OrdinalIgnoreCase))
            expenseArguments = message.Text[4..];
        else if (message.Text.StartsWith("log", StringComparison.OrdinalIgnoreCase))
            expenseArguments = message.Text[3..];
        else
            throw new InvalidUserInputException($"Message text doesn't start with {CommandName} command.");

        expenseArguments = expenseArguments.Trim();

        if (String.IsNullOrEmpty(expenseArguments))
        {
            await chatStateService.SetStateAsync(message.Chat.Id, "AwaitingLogArguments"); // TODO: create enum for chat states

            return await sender.ReplyAsync(message.Chat,
                "Okay, please enter the details for your expense. e.g. 'Café 5,50 Comida'",
                "Chat state: AwaitingLogArguments.",
                cancellationToken: cancellationToken);
        }

        return await LogExpenseAsync(message.Chat, expenseArguments, cancellationToken);
    }

    public async Task<Message> HandleLogAsync(Message message, ChatState chatState, CancellationToken cancellationToken)
    {
        await chatStateService.ClearState(message.Chat.Id);

        if (chatState.State == "AwaitingLogArguments")
        {
            return await LogExpenseAsync(message.Chat, message.Text, cancellationToken);
        }

        return await sender.ReplyAsync(message.Chat.Id, $"Log state {chatState.State} not implemented.",
            $"Log state {chatState} not implemented.",
            logLevel: LogLevel.Error,
            cancellationToken: cancellationToken
        );
    }

    private async Task<Message> LogExpenseAsync(ChatId chatId, string expenseArguments, CancellationToken cancellationToken)
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
        var match = Regex.Match(expenseArguments, @"^(.+)\s+([\d.,]+)\s*(.*)$");

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
}