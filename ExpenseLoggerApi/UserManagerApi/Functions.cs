using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using SharedLibrary.UserClasses;
using UserManagerApi.Common;
using UserManagerApi.Model;

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
    [LambdaFunction(Policies = "AWSLambdaBasicExecutionRole, DB_user_CRUD")]
    [HttpApi(LambdaHttpMethod.Post, "/user/signup")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> SignupUserAsync(
        [FromBody] UserSignupRequest request,
        ILambdaContext context)
    {
        context.Logger.LogInformation($"Signup attempt for TelegramId: {request.TelegramId}");

        var existingUser = await FindUserByTelegramIdAsync(request.TelegramId, context);

        if (existingUser != null)
        {
            context.Logger.LogInformation($"User already exists with UserId: {existingUser.UserId}");
            return ApiResponse.Ok(new UserExistsResponse { Success = false, UserId = existingUser.UserId });
        }

        context.Logger.LogInformation("User not found. Creating new user.");
        var newUser = await CreateNewUserAsync(request.TelegramId, request.Username, context);

        return ApiResponse.Created(
            $"/user/{newUser.UserId}", // TODO: Decide on canonical location header for user resource
            new UserResponse
            {
                Success = true,
                User = newUser
            });
    }

    /// <summary>
    /// Retrieves a user by their Telegram ID. Exposed as an API endpoint.
    /// </summary>
    /// <param name="telegramId">User's Telegram ID.</param>
    /// <param name="request">The API Gateway request containing the Telegram ID in the path.</param>
    /// <param name="context">Lambda context.</param>
    /// <returns>HTTP result containing the user if found, or NotFound/BadRequest.</returns>
    [LambdaFunction(Policies = "AWSLambdaBasicExecutionRole, DB_user_Read")] // Read-only access needed
    [HttpApi(LambdaHttpMethod.Get, "/user/telegram/{telegramId}")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> GetUserByTelegramIdAsync(
        string telegramId,
        APIGatewayHttpApiV2ProxyRequest request,
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
                return ApiResponse.NotFound($"User not found with TelegramId: {telegramIdNumber}");
            }

            context.Logger.LogInformation($"Found user with UserId: {user.UserId} for TelegramId: {telegramIdNumber}");
            // Return only necessary user information, avoid exposing internal details if needed
            return ApiResponse.Ok(
                new UserResponse
                {
                    Success = true,
                    User = user // TODO: don't send all user information for safety
                });
        }
        catch (Exception ex)
        {
            // Catch exceptions from FindUserByTelegramIdAsync (e.g., DynamoDB errors)
            context.Logger.LogError($"Error retrieving user by TelegramId {telegramIdNumber}: {ex.Message}");
            // Return a generic server error response
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
                Limit = 1 // Optional: If you expect only one user per TelegramId
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
            // Let the caller handle the exception and decide on the response
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
            // Let the caller handle the exception and decide on the response
            throw;
        }
    }
}