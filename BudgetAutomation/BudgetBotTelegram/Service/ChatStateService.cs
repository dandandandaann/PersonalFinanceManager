using Amazon.DynamoDBv2.DataModel;
using BudgetBotTelegram.Enum;
using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Model;

namespace BudgetBotTelegram.Service
{
    public class ChatStateService(IDynamoDBContext dbContext, ILogger<ChatStateService> logger) : IChatStateService
    {
        public async Task<(bool hasState, ChatState? chatState)> HasState(long chatId)
        {
            var chatIdStr = chatId.ToString();
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

        public async Task<ChatState> SetStateAsync(long chatId, ChatStateEnum chatState, string commandName)
        {
            string chatIdStr = chatId.ToString();
             logger.LogInformation("Setting state for chatId {ChatId} to '{State}'.", chatIdStr, chatState);

            var newState = new ChatState
            {
                ChatId = chatIdStr,
                State = chatState.ToString(),
                ActiveCommand = commandName,
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