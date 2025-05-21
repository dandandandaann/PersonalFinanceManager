using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Service;
using SharedLibrary.Telegram;

namespace BudgetBotTelegram.Handler;

public class CommandHandler(
    ISenderGateway sender,
    IEnumerable<ICommand> commandImplementations
) : ICommandHandler
{
    public async Task<Message> HandleCommandAsync(Message message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Entities);
        ArgumentNullException.ThrowIfNull(message.Text);

        var commandEntity = message.Entities[0];

        // Ensure the command is at the beginning of the message
        if (commandEntity.Offset != 0)
        {
            return await sender.ReplyAsync(message.Chat, "Commands should be on the first position of the message.",
                cancellationToken: cancellationToken);
        }

        // Extract the command using the entity's length
        var commandFromMessage = message.Text.Substring(0, commandEntity.Length).Split('@')[0].ToLowerInvariant();

        var commands = commandImplementations.ToDictionary(
            cmd => $"/{cmd.CommandName}".ToLowerInvariant(),
            cmd => cmd
        );

        if (commands.TryGetValue(commandFromMessage, out var commandToExecute))
        {
            return await commandToExecute.HandleAsync(message, cancellationToken);
        }

        return await sender.ReplyAsync(
            message.Chat,
            "Command not recognized.",
            $"Unknown command '{commandFromMessage}'.", cancellationToken: cancellationToken);
    }
}