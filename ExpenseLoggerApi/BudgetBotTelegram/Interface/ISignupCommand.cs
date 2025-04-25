using BudgetBotTelegram.Model;
using Telegram.Bot.Types;

namespace BudgetBotTelegram.Interface;

/// <summary>
/// Interface for handling the signup command.
/// </summary>
public interface ISignupCommand
{
    /// <summary>
    /// Handles the initial /signup command message.
    /// </summary>
    Task<Message> HandleSignupAsync(Message message, CancellationToken cancellationToken);

    /// <summary>
    /// Handles subsequent messages when the chat is in a signup-related state.
    /// </summary>
    Task<Message> HandleSignupAsync(Message message, ChatState chatState, CancellationToken cancellationToken);
} 