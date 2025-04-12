using Telegram.Bot.Types;

namespace BudgetBotTelegram.Interface;


public interface ICancelCommand
{
    Task<Message> HandleCancelAsync(Message message, CancellationToken cancellationToken);
}