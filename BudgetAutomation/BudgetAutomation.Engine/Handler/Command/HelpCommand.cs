using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Model;
using BudgetAutomation.Engine.Service;
using SharedLibrary.Telegram;

namespace BudgetAutomation.Engine.Handler.Command;

public class HelpCommand(ISenderGateway sender) : ICommand
{
    public string CommandName => StaticCommandName;
    public static string StaticCommandName => "help";

    public async Task<Message> HandleAsync(Message message, CancellationToken cancellationToken = default)
    {
        return await sender.ReplyAsync(message.Chat,
            "Comando ainda não implementado.",
            cancellationToken: cancellationToken);
    }

    public Task<Message> HandleAsync(Message message, ChatState chatState, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}