using BudgetAutomation.Engine.Model;

namespace BudgetAutomation.Engine.Interface;

using SharedLibrary.Telegram;
using System.Threading;
using System.Threading.Tasks;

public interface ICommand
{
    /// <summary>
    /// The name of the command (e.g., "log", "signup") without the leading '/'.
    /// </summary>
    string CommandName { get; }

    /// <summary>
    /// Handles the execution of the command.
    /// </summary>
    /// <param name="message">The incoming Telegram message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response message to send back to the user.</returns>
    Task<Message> HandleAsync(Message message, CancellationToken cancellationToken);

    /// <summary>
    /// Handles the execution of the command.
    /// </summary>
    /// <param name="message">The incoming Telegram message.</param>
    /// <param name="chatState">The current chat state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response message to send back to the user.</returns>
    Task<Message> HandleAsync(Message message, ChatState chatState, CancellationToken cancellationToken);
}