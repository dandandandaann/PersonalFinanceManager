using SharedLibrary.Telegram;

namespace BudgetAutomation.Engine.Interface;

public interface ITextMessageHandler
{
    Task<Message> HandleTextMessageAsync(Message message, CancellationToken cancellationToken = default);
}