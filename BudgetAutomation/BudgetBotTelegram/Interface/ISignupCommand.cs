using SharedLibrary.Telegram;

namespace BudgetBotTelegram.Interface;

/// <summary>
/// Interface for handling the signup command.
/// </summary>
public interface ISignupCommand
{
    /// <summary>
    /// Handles the initial /signup command message.
    /// </summary>
    Task<Message> HandleSignupAsync(Message message, CancellationToken cancellationToken = default);
}