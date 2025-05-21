
using SharedLibrary.Telegram;

namespace BudgetAutomation.Engine.Interface;

public interface IUpdateHandler
{
    Task HandleUpdateAsync(Update update, CancellationToken cancellationToken = default);
}