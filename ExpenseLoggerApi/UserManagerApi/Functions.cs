using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.DynamoDBv2;
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
    /// <param name="dynamoDbClient">Injected DynamoDB client.</param>
    /// <param name="context">Lambda context.</param>
    /// <returns>HTTP result indicating success (Created or OK).</returns>
    [LambdaFunction(Policies = "AWSLambdaBasicExecutionRole, DB_user_CRUD")]
    [HttpApi(LambdaHttpMethod.Post, "/user/signup")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> SignupUserAsync(
        [FromBody] UserSignupRequest request,
        [FromServices] IAmazonDynamoDB dynamoDbClient,
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
            $"/user/{newUser.UserId}", // TODO: fix
            new UserResponse
            {
                Success = true,
                User = newUser
            });
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
            context.Logger.LogError($"Error querying DynamoDB by TelegramId: {ex.Message}");
            // TODO:  In a real app, handle this more gracefully (e.g., return 500 Internal Server Error)
            throw; // Re-throw for now, Lambda will handle it
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

        // Use DynamoDBContext to save the user object
        // This respects the [DynamoDBTable] attribute and attribute mappings on the User class
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
            // Handle error appropriately
            throw; // Re-throw for now
        }
    }
}