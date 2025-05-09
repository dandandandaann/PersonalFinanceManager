using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using SharedLibrary.Dto;
using SharedLibrary.UserClasses;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace UserManagerApi;

/// <summary>
/// Handles User management operations.
/// </summary>
public class Functions(IDynamoDBContext dbContext)
{
    private const string TelegramIdIndexName = "telegramId-index";

    /// <summary>
    /// Creates a new user based on their Telegram ID if they don't already exist.
    /// </summary>
    /// <param name="request">The signup request containing TelegramId and optional Username.</param>
    /// <param name="context">Lambda context.</param>
    /// <returns>HTTP result indicating success (Created or OK).</returns>
    [LambdaFunction(
        Policies = "AWSLambdaBasicExecutionRole, " +
                   "arn:aws:iam::795287297286:policy/DB_user_CRUD, " +
                   "arn:aws:iam::795287297286:policy/Configurations_Read",
        MemorySize = 128,
        Timeout = 10)]
    [HttpApi(LambdaHttpMethod.Post, "/user/signup")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> SignupUserAsync(
        [FromBody] UserSignupRequest request,
        ILambdaContext context)
    {
        context.Logger.LogInformation($"Signup attempt for TelegramId: {request.TelegramId}");

        try
        {
            var existingUser = await FindUserByTelegramIdAsync(request.TelegramId, context);

            if (existingUser != null)
            {
                context.Logger.LogInformation($"User already exists with UserId: {existingUser.UserId}");
                return ApiResponse.Ok(new UserExistsResponse { Success = false, UserId = existingUser.UserId });
            }

            context.Logger.LogInformation("User not found. Creating new user.");
            var newUser = await CreateNewUserAsync(request.TelegramId, request.Username, context);

            return ApiResponse.Created(
                $"/user/{newUser.UserId}",
                new UserResponse
                {
                    Success = true,
                    User = newUser
                });
        }
        catch (Exception ex)
        {
            context.Logger.LogError("Error retrieving user by TelegramId {TelegramId}: {ExceptionMessage}",
                request.TelegramId, ex.Message);
            return ApiResponse.InternalServerError("An error occurred while retrieving the user.");
        }
    }

    /// <summary>
    /// Retrieves a user by their Telegram ID. Exposed as an API endpoint.
    /// </summary>
    /// <param name="telegramId">User's Telegram ID.</param>
    /// <param name="context">Lambda context.</param>
    /// <returns>HTTP result containing the user if found, or NotFound/BadRequest.</returns>
    [LambdaFunction(
        Policies = "AWSLambdaBasicExecutionRole, " +
                   "arn:aws:iam::795287297286:policy/DB_user_Read, " +
                   "arn:aws:iam::795287297286:policy/Configurations_Read",
        MemorySize = 128,
        Timeout = 10)]
    [HttpApi(LambdaHttpMethod.Get, "/user/telegram/{telegramId}")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> GetUserByTelegramIdAsync(
        string telegramId,
        ILambdaContext context)
    {
        if (!long.TryParse(telegramId, out var telegramIdNumber))
        {
            context.Logger.LogWarning("Invalid or missing telegramId: {TelegramId}", telegramId);
            return ApiResponse.BadRequest("Invalid or missing TelegramId in path.");
        }

        context.Logger.LogInformation($"Attempting to find user by TelegramId: {telegramIdNumber}");

        try
        {
            var user = await FindUserByTelegramIdAsync(telegramIdNumber, context);

            if (user == null)
            {
                context.Logger.LogInformation($"User not found for TelegramId: {telegramIdNumber}");
                return ApiResponse.Ok(
                    new UserExistsResponse
                    {
                        Success = false
                    });
            }

            context.Logger.LogInformation($"Found user with UserId: {user.UserId} for TelegramId: {telegramIdNumber}");
            return ApiResponse.Ok(
                new UserExistsResponse
                {
                    Success = true,
                    UserId = user.UserId
                });
        }
        catch (Exception ex)
        {
            context.Logger.LogError("Error retrieving user by TelegramId {TelegramId}: {ExceptionMessage}",
                telegramIdNumber, ex.Message);
            return ApiResponse.InternalServerError("An error occurred while retrieving the user.");
        }
    }

    private async Task<User?> FindUserByTelegramIdAsync(long telegramId, ILambdaContext context)
    {
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
                Limit = 1
            };

            var search = dbContext.FromQueryAsync<User>(queryOperationConfig);

            var user = (await search.GetNextSetAsync())?.FirstOrDefault();

            if (user != null)
            {
                context.Logger.LogInformation($"Found user for TelegramId: {telegramId}. UserId: {user.UserId}");
                return user;
            }

            context.Logger.LogInformation($"No user found for TelegramId: {telegramId}");
            return null;
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error querying DynamoDB by TelegramId {telegramId}: {ex.Message}");
            throw;
        }
    }

    private async Task<User> CreateNewUserAsync(long telegramId, string? username, ILambdaContext context)
    {
        var newUser = new User
        {
            UserId = Guid.NewGuid().ToString(),
            TelegramId = telegramId,
            Username = username,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            context.Logger.LogInformation($"Saving new user item with UserId: {newUser.UserId}");
            await dbContext.SaveAsync(newUser);
            context.Logger.LogInformation($"Successfully created user with UserId: {newUser.UserId}");
            return newUser;
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error saving user to DynamoDB using DynamoDBContext: {ex.Message}");
            throw;
        }
    }
}