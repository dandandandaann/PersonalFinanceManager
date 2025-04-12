using BudgetBotTelegram.Model;
using Telegram.Bot.Types;

namespace BudgetBotTelegram.Interface;

public interface ILogCommand
{
    Task<Message> HandleLogAsync(Message message, CancellationToken cancellationToken);
    Task<Message> HandleLogAsync(Message message, ChatState chatState, CancellationToken cancellationToken);
}