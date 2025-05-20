using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using SharedLibrary.Model;

namespace UserManagerApi.Service;

public interface IUserService
{
    Task<User?> FindUserByTelegramIdAsync(long telegramId, ILambdaLogger logger);
    Task<User> CreateUserAsync(long telegramId, string? username, ILambdaLogger logger);
}

public class UserService(IDynamoDBContext dbContext) : IUserService
{
    private readonly IDynamoDBContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private const string TelegramIdIndexName = "telegramId-index"; // This could be injected via IOptions if it varies

    public async Task<User?> FindUserByTelegramIdAsync(long telegramId, ILambdaLogger logger)
    {
        logger.LogInformation("UserService: Attempting to find user by TelegramId: {TelegramId}", telegramId);
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
                ? $"UserService: Found user for TelegramId: {telegramId}. UserId: {user.UserId}"
                : $"UserService: No user found for TelegramId: {telegramId}");

            return user;
        }
        catch (Exception ex)
        {
            logger.LogError("UserService: Error querying DynamoDB by TelegramId {TelegramId}: {Exception}", telegramId, ex.ToString());
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
            "UserService: Attempting to create new user with UserId: {NewUserId} for TelegramId: {TelegramId}",
            newUser.UserId, telegramId);
        try
        {
            await _dbContext.SaveAsync(newUser);
            logger.LogInformation("UserService: Successfully created user with UserId: {NewUserId}", newUser.UserId);
            return newUser;
        }
        catch (Exception ex)
        {
            logger.LogError(
                "UserService: Error saving user to DynamoDB. UserId: {NewUserId}, TelegramId: {TelegramId}. " +
                "Exception: {Exception}", newUser.UserId, telegramId, ex.ToString());
            throw;
        }
    }
}