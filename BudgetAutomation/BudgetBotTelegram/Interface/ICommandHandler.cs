
using SharedLibrary.Telegram;

namespace BudgetBotTelegram.Interface;

public interface ICommandHandler
{
    Task<Message> HandleCommandAsync(Message message, CancellationToken cancellationToken = default);
}