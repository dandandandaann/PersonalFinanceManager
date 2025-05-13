

using SharedLibrary.Telegram;

namespace BudgetBotTelegram.Interface;


public interface ICancelCommand
{
    Task<Message> HandleCancelAsync(Message message, CancellationToken cancellationToken = default);
}