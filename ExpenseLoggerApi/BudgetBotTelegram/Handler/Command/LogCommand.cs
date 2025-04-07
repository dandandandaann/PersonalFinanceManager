using BudgetBotTelegram.ApiClient;
using BudgetBotTelegram.Model;
using Telegram.Bot.Types;

namespace BudgetBotTelegram.Handler.Command;

public class LogCommand(SenderGateway sender, ILogger<MessageHandler> logger, ExpenseLoggerApiClient expenseApiClient)
{
    private const string CommandName = "log";
    public async Task<Message> HandleLogAsync(Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Text);

        string logArguments, logMessage = string.Empty;

        if (message.Text.StartsWith("/log ", StringComparison.OrdinalIgnoreCase))
            logArguments = message.Text[5..];
        else if (message.Text.StartsWith("log ", StringComparison.OrdinalIgnoreCase))
            logArguments = message.Text[4..];
        else
            throw new ArgumentException($"Message text doesn't start with {CommandName} command.", nameof(message.Text));

        var argumentsSplit = logArguments.Trim().Split(' ');
        if (argumentsSplit.Length < 2)
        {
            throw new ArgumentException("Invalid message format for logging expense.");
        }

        if (!decimal.TryParse(argumentsSplit[1], out _))
        {
            await sender.ReplyAsync(message.Chat, "Invalid message format for logging expense.", logMessage, cancellationToken: cancellationToken);
        }

        var expense = new Expense
        {
            Description = argumentsSplit[0],
            Amount = argumentsSplit[1],
            Category = argumentsSplit.Length >= 3 ? argumentsSplit[2] : string.Empty
        };

        try
        {
            await expenseApiClient.LogExpenseAsync(expense, cancellationToken);
            logArguments = $"Logged Expense\n{expense}";
            logMessage = "Logged expense.";
        }
        catch (ArgumentException e)
        {
            logArguments = e.Message;
            logMessage = $"Argument Exception: {e.Message}.";
        }
        return await sender.ReplyAsync(message.Chat, logArguments, logMessage, cancellationToken: cancellationToken);
    }
}