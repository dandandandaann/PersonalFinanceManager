using Telegram.Bot.Types;

namespace BudgetBotTelegram.Interface;


public interface ILogCommand
{
    Task<Message> HandleLogAsync(Message message, CancellationToken cancellationToken);
}