using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Model;
using BudgetAutomation.Engine.Service;
using SharedLibrary.Telegram;

namespace BudgetAutomation.Engine.Handler.Command;

public class CancelCommand(ISenderGateway sender, IChatStateService chatStateService) : ICommand
{
    public string CommandName => "cancel";

    public async Task<Message> HandleAsync(Message message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Text);

        UserManagerService.EnsureUserSignedIn();

        await chatStateService.ClearState(message.Chat.Id);

        return await sender.ReplyAsync(message.Chat,
            "Comando de cancelamento concluído. \nO que você quer fazer a seguir?", cancellationToken: cancellationToken);
    }

    public Task<Message> HandleAsync(Message message, ChatState chatState, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}