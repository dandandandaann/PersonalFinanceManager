using BudgetBotTelegram.Interface;
using Telegram.Bot.Types;

namespace BudgetBotTelegram.Handler.Command;

public class CancelCommand(ISenderGateway sender, ChatStateService chatStateService) : ICancelCommand
{
    public const string CommandName = "cancel";

    public async Task<Message> HandleCancelAsync(Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Text);

        // TODO: clear chat state

        await chatStateService.ClearState(message.Chat.Id);

        return await sender.ReplyAsync(message.Chat,
            "Cancel command done. \nWhat do you want to do next?", cancellationToken: cancellationToken);
    }
}