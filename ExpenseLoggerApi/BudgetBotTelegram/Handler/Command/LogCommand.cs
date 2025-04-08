using System.Text.RegularExpressions;
using BudgetBotTelegram.ApiClient;
using BudgetBotTelegram.Model;
using Telegram.Bot.Types;

namespace BudgetBotTelegram.Handler.Command;

public interface ILogCommand
{
    Task<Message> HandleLogAsync(Message message, CancellationToken cancellationToken);
}

public class LogCommand(ISenderGateway sender, IExpenseLoggerApiClient expenseApiClient) : ILogCommand
{
    private const string CommandName = "log";

    public async Task<Message> HandleLogAsync(Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Text);

        string expenseArguments, logMessage = string.Empty;
        var expense = new Expense();

        if (message.Text.StartsWith("/log ", StringComparison.OrdinalIgnoreCase))
            expenseArguments = message.Text[5..];
        else if (message.Text.StartsWith("log ", StringComparison.OrdinalIgnoreCase))
            expenseArguments = message.Text[4..];
        else
            throw new ArgumentException($"Message text doesn't start with {CommandName} command.", nameof(message.Text));

        expenseArguments = expenseArguments.Trim();

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
                throw new ArgumentException("Invalid message format for logging expense.");

            expense.Description = argumentsSplit[0];
            expense.Amount = argumentsSplit[1];
            expense.Category = argumentsSplit.Length >= 3 ? argumentsSplit[2] : string.Empty;
        }

        if (!decimal.TryParse(expense.Amount, out _))
        {
            return await sender.ReplyAsync(
                message.Chat,
                "Invalid message format for logging expense.",
                logMessage,
                cancellationToken: cancellationToken);
        }

        try
        {
            await expenseApiClient.LogExpenseAsync(expense, cancellationToken);
            expenseArguments = $"Logged Expense\n{expense}";
            logMessage = "Logged expense.";
        }
        catch (ArgumentException e)
        {
            expenseArguments = e.Message;
            logMessage = $"Argument Exception: {e.Message}.";
        }

        return await sender.ReplyAsync(message.Chat, expenseArguments, logMessage, cancellationToken: cancellationToken);
    }
}