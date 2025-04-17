using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Model;
using BudgetBotTelegram.Service;
using BudgetBotTelegram.Settings;
using Telegram.Bot.Types;

namespace BudgetBotTelegram.Handler;

public class TextMessageHandler(
    ISenderGateway sender,
    ILogCommand log,
    IChatStateService chatStateService,
    ICancelCommand cancel) : ITextMessageHandler
{
    public async Task<Message> HandleTextMessageAsync(Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Text);
        var messageText = message.Text;

        if (messageText.StartsWith("log ", StringComparison.CurrentCultureIgnoreCase) || messageText == "log")
            return await log.HandleLogAsync(message, cancellationToken);

        if (messageText.StartsWith("cancel", StringComparison.CurrentCultureIgnoreCase) || messageText == "cancel")
            return await cancel.HandleCancelAsync(message, cancellationToken);

        (bool hasState, ChatState chatState) = await chatStateService.HasState(message.Chat.Id);
        if (!hasState) // Default message
            return await sender.ReplyAsync(message.Chat, "You said:\n" + messageText, cancellationToken: cancellationToken);

        if (chatState?.State == "AwaitingLogArguments")
            return await log.HandleLogAsync(message, chatState, cancellationToken);

        // TODO: handle state
        throw new NotImplementedException();
    }
}