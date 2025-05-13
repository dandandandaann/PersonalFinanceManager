using SharedLibrary.Telegram;

namespace BudgetBotTelegram.Interface;

public interface ITextMessageHandler
{
    Task<Message> HandleTextMessageAsync(Message message, CancellationToken cancellationToken = default);
}