using Telegram.Bot.Types;

namespace BudgetBotTelegram.Interface;

public interface ITextMessageHandler
{
    Task HandleTextMessageAsync(Message message, CancellationToken cancellationToken);
}