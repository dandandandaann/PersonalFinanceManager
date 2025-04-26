using Telegram.Bot.Types;

namespace BudgetBotTelegram.Interface;

public interface IMessageHandler
{
    Task HandleMessageAsync(Message message, CancellationToken cancellationToken = default);
}