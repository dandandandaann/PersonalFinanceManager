using BudgetBotTelegram.Handler.Command;
using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Model;
using BudgetBotTelegram.Service;
using SharedLibrary.Telegram;

namespace BudgetBotTelegram.Handler;

public class TextMessageHandler(
    ILogger<MessageHandler> logger,
    ISenderGateway sender,
    IChatStateService chatStateService,
    ILogCommand logCommand,
    ISignupCommand signupCommand,
    ICancelCommand cancelCommand) : ITextMessageHandler
{
    public async Task<Message> HandleTextMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Text);
        var messageText = message.Text;

        if (messageText.StartsWith($"{LogCommand.CommandName} ", StringComparison.CurrentCultureIgnoreCase) ||
            messageText.Equals(LogCommand.CommandName, StringComparison.CurrentCultureIgnoreCase))
            return await logCommand.HandleLogAsync(message, cancellationToken);

        if (messageText.Equals(SignupCommand.CommandName, StringComparison.CurrentCultureIgnoreCase))
            return await signupCommand.HandleSignupAsync(message, cancellationToken);

        if (messageText.Equals(CancelCommand.CommandName, StringComparison.CurrentCultureIgnoreCase))
            return await cancelCommand.HandleCancelAsync(message, cancellationToken);

        if (!UserManagerService.UserLoggedIn)
            throw new UnauthorizedAccessException();

        (bool hasState, ChatState? chatState) = await chatStateService.HasState(message.Chat.Id);
        if (!hasState) // Default message
            return await sender.ReplyAsync(message.Chat, "You said:\n" + messageText, cancellationToken: cancellationToken);

        if (chatState?.State == ChatStateService.StateEnum.AwaitingLogArguments.ToString())
            return await logCommand.HandleLogAsync(message, chatState, cancellationToken);

        // TODO: handle state
        logger.LogError("Chat state is null or not implemented: {ChatState}", chatState?.State);
        throw new NotImplementedException();
    }
}