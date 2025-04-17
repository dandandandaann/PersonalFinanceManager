using Amazon.DynamoDBv2.DataModel;
using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Model;

namespace BudgetBotTelegram.Service
{
    public class ChatStateService(IDynamoDBContext dbContext, ILogger<ChatStateService> logger) : IChatStateService
    {
        /// <summary>
        /// Checks if a chat state exists for the given chatId and retrieves it.
        /// </summary>
        /// <param name="chatId">The Telegram chat ID.</param>
        /// <returns>A tuple indicating if state exists and the ChatState object (null if not found).</returns>
        public async Task<(bool hasState, ChatState? chatState)> HasState(long chatId)
        {
            string chatIdStr = chatId.ToString();
            // Table name is inferred from the [DynamoDBTable] attribute on ChatState
            logger.LogInformation("Checking state for chatId {ChatId}.", chatIdStr);

            try
            {
                ChatState? state = await dbContext.LoadAsync<ChatState>(chatIdStr, chatIdStr);

                if (state != null)
                {
                    logger.LogInformation("State found for chatId {ChatId}: {State}.", chatIdStr, state.State ?? "null");
                    return (true, state);
                }

                logger.LogInformation("No state found for chatId {ChatId}.", chatIdStr);
                return (false, null);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking state for chatId {ChatId}.", chatIdStr);
                throw; // Re-throw exceptions to be handled upstream
            }
        }

        /// <summary>
        /// Clears the chat state for the given chatId by deleting the item.
        /// </summary>
        /// <param name="chatId">The Telegram chat ID.</param>
        /// <returns>The ChatState object that was deleted, or null if no state existed.</returns>
        public async Task<ChatState?> ClearState(long chatId)
        {
            string chatIdStr = chatId.ToString();
            logger.LogInformation("Attempting to clear state for chatId {ChatId}.", chatIdStr);

            try
            {
                // Load the existing state first to return it (as per your method signature)
                ChatState? existingState = await dbContext.LoadAsync<ChatState>(chatIdStr, chatIdStr);

                if (existingState != null)
                {
                    await dbContext.DeleteAsync(existingState); // TODO: can I delete without loading it first?
                    // Or: await _dbContext.DeleteAsync<ChatState>(chatIdStr, chatIdStr);

                    logger.LogInformation("Successfully cleared state for chatId {ChatId}.", chatIdStr);
                    return existingState; // Return the state that was just deleted
                }
                else
                {
                    logger.LogInformation("No state found to clear for chatId {ChatId}.", chatIdStr);
                    return null; // Nothing to clear
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error clearing state for chatId {ChatId}.", chatIdStr);
                throw;
            }
        }

        /// <summary>
        /// Sets (creates or updates) the chat state for the given chatId.
        /// </summary>
        /// <param name="chatId">The Telegram chat ID.</param>
        /// <param name="stateValue">The new state value.</param>
        /// <returns>The newly created or updated ChatState object.</returns>
        public async Task<ChatState> SetStateAsync(long chatId, string stateValue)
        {
            string chatIdStr = chatId.ToString();
             logger.LogInformation("Setting state for chatId {ChatId} to '{State}'.", chatIdStr, stateValue);

            var newState = new ChatState
            {
                ChatId = chatIdStr,
                State = stateValue,
                Timestamp = DateTime.UtcNow
            };

            try
            {
                // SaveAsync performs an "upsert".
                await dbContext.SaveAsync(newState);
                logger.LogInformation("Successfully set state for chatId {ChatId}.", chatIdStr);
                return newState;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error setting state for chatId {ChatId}.", chatIdStr);
                throw;
            }
        }
    }
}