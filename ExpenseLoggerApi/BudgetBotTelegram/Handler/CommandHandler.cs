using BudgetBotTelegram.Handler.Command;
using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Service;
using Telegram.Bot.Types;

namespace BudgetBotTelegram.Handler;

public class CommandHandler(ISenderGateway sender, ILogCommand log, ICancelCommand cancel) : ICommandHandler
{
    public async Task<Message> HandleCommandAsync(Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Entities);
        ArgumentNullException.ThrowIfNull(message.Text);

        var commandEntity = message.Entities[0];

        // Ensure the command is at the beginning of the message
        if (commandEntity.Offset != 0)
        {
            return await sender.ReplyAsync(message.Chat, "Commands should be on the first position of the message.", cancellationToken: cancellationToken);
        }

        // Extract the command using the entity's length
        var command = message.Text.Substring(0, commandEntity.Length).Split('@')[0];

        if (command.Equals($"/{LogCommand.CommandName}", StringComparison.OrdinalIgnoreCase))
            return await log.HandleLogAsync(message, cancellationToken);

        if (command.Equals($"/{CancelCommand.CommandName}", StringComparison.OrdinalIgnoreCase))
            return await cancel.HandleCancelAsync(message, cancellationToken);

        return await sender.ReplyAsync(
            message.Chat,
            "Command not recognized.",
            $"Unknown command '{command}'.", cancellationToken: cancellationToken);
    }
}