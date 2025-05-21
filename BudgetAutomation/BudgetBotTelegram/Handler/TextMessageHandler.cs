using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Model;
using BudgetBotTelegram.Service;
using SharedLibrary.Telegram;

namespace BudgetBotTelegram.Handler;

public class TextMessageHandler(
    ILogger<MessageHandler> logger,
    ISenderGateway sender,
    IChatStateService chatStateService,
    IEnumerable<ICommand> commandImplementations) : ITextMessageHandler
{
    public async Task<Message> HandleTextMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Text);
        var messageText = message.Text;

        // TODO: this is odd, try to change it
        string[] parts = messageText.Split([' '], 2, StringSplitOptions.RemoveEmptyEntries);

        var commandsByName = commandImplementations.ToDictionary(
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

        if (!UserManagerService.UserLoggedIn)
            throw new UnauthorizedAccessException();

        (bool hasState, ChatState? chatState) = await chatStateService.HasState(message.Chat.Id);

        if (!hasState) // Default message
            return await sender.ReplyAsync(message.Chat, "You said:\n" + messageText, cancellationToken: cancellationToken);

        if (chatState?.ActiveCommand != null)
        {
            if (commandsByName.TryGetValue(chatState.ActiveCommand.ToLowerInvariant(), out var statefulCommand))
            {
                logger.LogInformation("Handling text as continuation for command: {CommandName}, State: {State}",
                    statefulCommand.CommandName, chatState.State);
                return await statefulCommand.HandleAsync(message, chatState, cancellationToken);
            }
        }

        // TODO: handle state
        logger.LogError("Chat state is null or not implemented: {ChatState}", chatState?.State);
        throw new NotImplementedException();
    }
}