
using SharedLibrary.Telegram;

namespace BudgetAutomation.Engine.Interface;

public interface ICommandHandler
{
    Task<Message> HandleCommandAsync(Message message, CancellationToken cancellationToken = default);
}