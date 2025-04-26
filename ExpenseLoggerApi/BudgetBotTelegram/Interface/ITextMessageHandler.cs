using Telegram.Bot.Types;

namespace BudgetBotTelegram.Interface;

public interface ITextMessageHandler
{
    Task<Message> HandleTextMessageAsync(Message message, CancellationToken cancellationToken = default);
}