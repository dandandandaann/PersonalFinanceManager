using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Service;
using SharedLibrary.Telegram;


namespace BudgetBotTelegram.Handler.Command;

public class CancelCommand(ISenderGateway sender, IChatStateService chatStateService) : ICancelCommand
{
    public const string CommandName = "cancel";

    public async Task<Message> HandleCancelAsync(Message message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Text);

        if (!UserManagerService.UserLoggedIn)
            throw new UnauthorizedAccessException();

        await chatStateService.ClearState(message.Chat.Id);

        return await sender.ReplyAsync(message.Chat,
            "Cancel command done. \nWhat do you want to do next?", cancellationToken: cancellationToken);
    }
}