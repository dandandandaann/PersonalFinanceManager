
using SharedLibrary.Telegram;

namespace BudgetBotTelegram.Interface;

public interface IUpdateHandler
{
    Task HandleUpdateAsync(Update update, CancellationToken cancellationToken = default);
}