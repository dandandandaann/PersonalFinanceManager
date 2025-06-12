using BudgetAutomation.Engine.Handler.Command;
using BudgetAutomation.Engine.Handler.Command.Alias;
using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Model;
using BudgetAutomation.Engine.Service;
using SharedLibrary.Telegram;

namespace BudgetAutomation.Engine.Handler;

public class TextMessageHandler(
    ILogger<MessageHandler> logger,
    ISenderGateway sender,
    IChatStateService chatStateService,
    IEnumerable<ICommand> commandImplementations,
    IEnumerable<CommandAliasBase> commandAliasImplementations) : ITextMessageHandler
{
    private static readonly string[] CommandsAllowedAsPlainText = [LogCommand.StaticCommandName];

    public async Task<Message> HandleTextMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Text);
        var messageText = message.Text;

        string[] parts = messageText.Split([' '], 2, StringSplitOptions.RemoveEmptyEntries);

        var commandsByName =
            commandImplementations.Concat(commandAliasImplementations)
                .Where(x => CommandsAllowedAsPlainText.Contains(x.CommandName))
                .ToDictionary(
                    cmd => cmd.CommandName.ToLowerInvariant(),
                    cmd => cmd
                );

        if (parts.Length > 0)
        {
            var potentialCommandName = parts[0].ToLowerInvariant();
            commandsByName.TryGetValue(potentialCommandName, out var commandToExecute);

            // If a command is identified directly from the text
            if (commandToExecute != null)
            {
                logger.LogInformation("Handling text as direct command: {CommandName}", commandToExecute.CommandName);
                return await commandToExecute.HandleAsync(message, cancellationToken);
            }
        }

        UserManagerService.EnsureUserSignedIn();

        (bool hasState, ChatState? chatState) = await chatStateService.HasState(message.Chat.Id);

        if (!hasState) // Default message
        {
            var replyMessage =
                await sender.ReplyAsync(message.Chat, "Comando não reconhecido.", cancellationToken: cancellationToken);

            if (commandsByName.TryGetValue(StartCommand.StaticCommandName, out var startCommand))
            {
                logger.LogInformation("Default response with {CommandName} command.", startCommand.CommandName);
                return await startCommand.HandleAsync(message, cancellationToken);
            }

            logger.LogError("Not able to find {CommandName} command to send as default message.", StartCommand.StaticCommandName);
            return replyMessage;
        }

        if (chatState?.ActiveCommand != null)
        {
            if (commandsByName.TryGetValue(chatState.ActiveCommand.ToLowerInvariant(), out var commandSavedInState))
            {
                logger.LogInformation("Handling text as continuation for command: {CommandName}, State: {State}",
                    commandSavedInState.CommandName, chatState.State);
                return await commandSavedInState.HandleAsync(message, chatState, cancellationToken);
            }
        }

        // TODO: handle state
        logger.LogError("Chat state is null or not implemented: {ChatState}", chatState?.State);
        throw new NotImplementedException();
    }
}