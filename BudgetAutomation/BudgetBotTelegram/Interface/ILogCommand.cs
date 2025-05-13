using BudgetBotTelegram.Model;
using SharedLibrary.Telegram;

namespace BudgetBotTelegram.Interface;

public interface ILogCommand
{
    Task<Message> HandleLogAsync(Message message, CancellationToken cancellationToken = default);
    Task<Message> HandleLogAsync(Message message, ChatState chatState, CancellationToken cancellationToken = default);
}