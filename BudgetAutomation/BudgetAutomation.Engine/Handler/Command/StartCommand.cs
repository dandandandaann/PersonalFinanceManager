using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Model;
using BudgetAutomation.Engine.Service;
using SharedLibrary.Telegram;

namespace BudgetAutomation.Engine.Handler.Command;

public class StartCommand(ISenderGateway sender, IChatStateService chatStateService) : ICommand
{
    public string CommandName => StaticCommandName;
    public static string StaticCommandName => "start";

    public Task<Message> HandleAsync(Message message, CancellationToken cancellationToken = default)
    {
        // TODO: register this class in the DI service collection and finish implementation
        throw new NotImplementedException();
    }

    public Task<Message> HandleAsync(Message message, ChatState chatState, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}