using BudgetBotTelegram.Model;

namespace BudgetBotTelegram.Interface;

public interface IChatStateService
{
    /// <summary>
    /// Checks if a chat state exists for the given chatId and retrieves it.
    /// </summary>
    /// <param name="chatId">The Telegram chat ID.</param>
    /// <returns>A tuple indicating if state exists and the ChatState object (null if not found).</returns>
    Task<(bool hasState, ChatState? chatState)> HasState(long chatId);

    /// <summary>
    /// Clears the chat state for the given chatId by deleting the item.
    /// </summary>
    /// <param name="chatId">The Telegram chat ID.</param>
    /// <returns>The ChatState object that was deleted, or null if no state existed.</returns>
    Task<ChatState?> ClearState(long chatId);

    /// <summary>
    /// Sets (creates or updates) the chat state for the given chatId.
    /// </summary>
    /// <param name="chatId">The Telegram chat ID.</param>
    /// <param name="stateValue">The new state value.</param>
    /// <returns>The newly created or updated ChatState object.</returns>
    Task<ChatState> SetStateAsync(long chatId, string stateValue);
}