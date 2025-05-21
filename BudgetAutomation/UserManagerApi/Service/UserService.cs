using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using SharedLibrary.Model;

namespace UserManagerApi.Service;

public interface IUserService
{
    Task<User?> FindUserByTelegramIdAsync(long telegramId, ILambdaLogger logger);
    Task<User> CreateUserAsync(long telegramId, string? username, ILambdaLogger logger);
    Task<User?> GetUserAsync(string userId, ILambdaLogger logger);
    Task<User?> UpdateUserConfigurationAsync(string userId, UserConfiguration userConfiguration, ILambdaLogger logger);
    Task<User> UpsertUserAsync(User user, ILambdaLogger logger);
}

public class UserService(IDynamoDBContext dbContext) : IUserService
{
    private readonly IDynamoDBContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private const string TelegramIdIndexName = "telegramId-index"; // This could be injected via IOptions if it varies

    public async Task<User?> FindUserByTelegramIdAsync(long telegramId, ILambdaLogger logger)
    {
        logger.LogInformation("UpsertUserAsync: Attempting to find user by TelegramId: {TelegramId}", telegramId);
        try
        {
            var queryOperationConfig = new QueryOperationConfig
            {
                IndexName = TelegramIdIndexName,
                KeyExpression = new Expression
                {
                    ExpressionStatement = "telegramId = :v_telegramId",
                    ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                    {
                        { ":v_telegramId", telegramId }
                    }
                },
                Limit = 1 // We only expect one user or none
            };

            var search = _dbContext.FromQueryAsync<User>(queryOperationConfig);
            var users = await search.GetNextSetAsync();
            var user = users?.FirstOrDefault();

            logger.LogInformation(user != null
                ? $"UpsertUserAsync: Found user for TelegramId: {telegramId}. UserId: {user.UserId}"
                : $"UpsertUserAsync: No user found for TelegramId: {telegramId}");

            return user;
        }
        catch (Exception ex)
        {
            logger.LogError("UpsertUserAsync: Error querying DynamoDB by TelegramId {TelegramId}: {Exception}", telegramId, ex.ToString());
            throw;
        }
    }

    public async Task<User> CreateUserAsync(long telegramId, string? username, ILambdaLogger logger)
    {
        var newUser = new User
        {
            UserId = Guid.NewGuid().ToString(),
            TelegramId = telegramId,
            Username = username,
            CreatedAt = DateTime.UtcNow
        };

        logger.LogInformation(
            "CreateUserAsync: Attempting to create new user with UserId: {NewUserId} for TelegramId: {TelegramId}",
            newUser.UserId, telegramId);

        return await UpsertUserAsync(newUser, logger);
    }

    public async Task<User?> GetUserAsync(string userId, ILambdaLogger logger)
    {
        logger.LogInformation("GetUserAsync: Attempting to get user by UserId: {UserId}", userId);
        try
        {
            var user = await _dbContext.LoadAsync<User>(userId, userId);

            if (user == null)
            {
                logger.LogWarning("GetUserAsync: No user found for UserId: {UserId}", userId);
                return null;
            }

            logger.LogInformation("GetUserAsync: Successfully retrieved user with UserId: {UserId}", userId);
            return user;
        }
        catch (KeyNotFoundException)
        {
            logger.LogWarning("GetUserAsync: No key found for UserId: {UserId}", userId);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError("GetUserAsync: Error retrieving user from DynamoDB by UserId {UserId}: {Exception}", userId, ex.ToString());
            throw;
        }
    }

    public async Task<User?> UpdateUserConfigurationAsync(string userId, UserConfiguration userConfiguration, ILambdaLogger logger)
    {
        var user = await GetUserAsync(userId, logger);

        if (user == null)
        {
            logger.LogInformation("UpdateUserConfigurationAsync: User not found: {UserId}", userId);
            return null;
        }

        user.Configuration = userConfiguration;

        return await UpsertUserAsync(user, logger);
    }

    public async Task<User> UpsertUserAsync(User user, ILambdaLogger logger)
    {
        ArgumentNullException.ThrowIfNull(user);

        try
        {
            await _dbContext.SaveAsync(user);
            logger.LogInformation("UpsertUserAsync: Successfully upserted user with UserId: {UserId}", user.UserId);
            return user;
        }
        catch (Exception ex)
        {
            logger.LogError(
                "UpsertUserAsync: Error saving user to DynamoDB. UserId: {UserId}, TelegramId: {TelegramId}. " +
                "Exception: {Exception}", user.UserId, user.TelegramId, ex.ToString());
            throw;
        }
    }
}