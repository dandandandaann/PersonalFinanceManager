
using SharedLibrary.Telegram;

namespace BudgetAutomation.Engine.Interface;

public interface IMessageHandler
{
    Task HandleMessageAsync(Message message, CancellationToken cancellationToken = default);
}