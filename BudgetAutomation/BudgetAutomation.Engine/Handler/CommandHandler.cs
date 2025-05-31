using BudgetAutomation.Engine.Handler.Command;
using BudgetAutomation.Engine.Handler.Command.Alias;
using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Service;
using SharedLibrary.Telegram;

namespace BudgetAutomation.Engine.Handler;

public class CommandHandler(
    ISenderGateway sender,
    IEnumerable<ICommand> commandImplementations,
    IEnumerable<CommandAliasBase> commandAliasImplementations
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
            return await sender.ReplyAsync(message.Chat, "Os comandos devem estar na primeira posição da mensagem.",
                cancellationToken: cancellationToken);
        }

        // Extract the command using the entity's length
        var commandFromMessage = message.Text.Substring(0, commandEntity.Length).Split('@')[0].ToLowerInvariant();

        var commandsAndAliases = commandImplementations.Concat(commandAliasImplementations).ToDictionary(
            cmd => $"/{cmd.CommandName}".ToLowerInvariant(),
            cmd => cmd
        );

        if (commandsAndAliases.TryGetValue(commandFromMessage, out var commandToExecute))
        {
            return await commandToExecute.HandleAsync(message, cancellationToken);
        }

        UserManagerService.EnsureUserSignedIn();

        await sender.ReplyAsync(
            message.Chat,
            "Comando não reconhecido.",
            $"Unknown command '{commandFromMessage}'.", cancellationToken: cancellationToken);

        return await commandsAndAliases.First(x => x.Key == $"/{StartCommand.StaticCommandName}")
            .Value.HandleAsync(message, cancellationToken);

    }
}