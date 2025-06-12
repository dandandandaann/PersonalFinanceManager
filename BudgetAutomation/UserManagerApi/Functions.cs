using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using SharedLibrary.Dto;
using SharedLibrary.Model;
using SharedLibrary.Enum;
using UserManagerApi.Service;
using Results = SharedLibrary.Lambda.Results;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace UserManagerApi;

/// <summary>
/// Handles User management operations.
/// </summary>
/// <param name="userService">User service from injection dependency.</param>
public class Functions(IUserService userService)
{
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
        [FromBody] UserSignupRequest request, ILambdaContext context)
    {
        var logger = context.Logger;
        logger.LogInformation("SignupUserAsync: Received signup request for TelegramId: {TelegramId}", request.TelegramId);


        if (request.TelegramId <= 0) // Basic validation
        {
            logger.LogWarning("SignupUserAsync: Invalid TelegramId received: {TelegramId}", request.TelegramId);
            return Results.BadRequest(new UserSignupResponse
            {
                Success = false,
                Message = "Invalid TelegramId.",
                ErrorCode = ErrorCodeEnum.InvalidInput
            });
        }

        try
        {
            var existingUser = await userService.FindUserByTelegramIdAsync(request.TelegramId, logger);

            if (existingUser != null)
            {
                logger.LogInformation(
                    "SignupUserAsync: User already exists with UserId: {ExistingUserId} for TelegramId: {TelegramId}",
                    existingUser.UserId, request.TelegramId);
                return Results.Ok(
                    new UserSignupResponse
                    {
                        Success = false,
                        Message = "User already exists.",
                        User = new User { UserId = existingUser.UserId, TelegramId = request.TelegramId },
                        ErrorCode = ErrorCodeEnum.UserAlreadyExists
                    });
            }

            logger.LogInformation("SignupUserAsync: User not found for TelegramId: {TelegramId}. Creating new user.",
                request.TelegramId);
            var newUser = await userService.CreateUserAsync(
                request.TelegramId, request.Email, request.Username, logger
            );

            logger.LogInformation("SignupUserAsync: Successfully created new user with UserId: {NewUserId}",
                newUser.UserId);
            return Results.Created(
                $"/user/{newUser.UserId}",
                new UserSignupResponse
                {
                    Success = true,
                    User = newUser,
                    Message = "User created successfully."
                });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SignupUserAsync: Error during signup for TelegramId {TelegramId}.",
                request.TelegramId);
            return Results.InternalServerError(new UserSignupResponse
            {
                Success = false,
                Message = "An error occurred during the signup process.",
                ErrorCode = ErrorCodeEnum.InternalError
            });
        }
    }

    /// <summary>
    /// Creates a new user based on their Telegram ID if they don't already exist.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request">The signup request containing TelegramId and optional Username.</param>
    /// <param name="context">Lambda context.</param>
    /// <returns>HTTP result indicating success (Created or OK).</returns>
    [LambdaFunction(
        Policies = "AWSLambdaBasicExecutionRole, " +
                   "arn:aws:iam::795287297286:policy/DB_user_CRUD, " +
                   "arn:aws:iam::795287297286:policy/Configurations_Read",
        MemorySize = 128,
        Timeout = 10)]
    [HttpApi(LambdaHttpMethod.Put, "/user/{userId}/configuration")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> UpdateUserConfigurationAsync(
        string userId, [FromBody] UserConfigurationUpdateRequest request, ILambdaContext context)
    {
        var logger = context.Logger;
        logger.LogInformation("UpdateUserConfigurationAsync: Received request for UserId: {UserId}", userId);

        if (string.IsNullOrWhiteSpace(userId))
        {
            logger.LogWarning("UpdateUserConfigurationAsync: Invalid UserId received in path: {UserId}", userId);
            return Results.BadRequest(new UserConfigurationUpdateResponse
            {
                Success = false,
                Message = "Invalid UserId provided in path.",
                ErrorCode = ErrorCodeEnum.InvalidInput
            });
        }

        try
        {
            var updatedUser = await userService.UpdateUserConfigurationAsync(userId,
                new UserConfiguration { SpreadsheetId = request.UserConfiguration.SpreadsheetId }, logger
            );

            if (updatedUser == null)
            {
                logger.LogInformation("UpdateUserConfigurationAsync: User not found: {UserId}", userId);
                return Results.Ok(
                    new UserGetResponse
                    {
                        Success = false,
                        Message = "User not found.",
                        ErrorCode = ErrorCodeEnum.ResourceNotFound
                    });
            }

            logger.LogInformation("UpdateUserConfigurationAsync: User configuration updated was successful: {UserId}", userId);

            return Results.Ok(new UserConfigurationUpdateResponse
                { Success = true, Message = "User configuration updated." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UpdateUserConfigurationAsync: Error during update for UserId {UserId}.", userId);
            return Results.InternalServerError("An error occurred during the configuration update.");
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
        string telegramId, ILambdaContext context)
    {
        var logger = context.Logger;
        logger.LogInformation("GetUserByTelegramIdAsync: Received request for TelegramId: {TelegramId}", telegramId);

        if (!long.TryParse(telegramId, out var telegramIdNumber) || telegramIdNumber <= 0)
        {
            logger.LogWarning("GetUserByTelegramIdAsync: Invalid TelegramId format or value: {TelegramId}", telegramId);
            return Results.BadRequest(new UserGetResponse
            {
                Success = false,
                Message = "Invalid TelegramId format or value provided.",
                ErrorCode = ErrorCodeEnum.InvalidInput
            });
        }

        try
        {
            var user = await userService.FindUserByTelegramIdAsync(telegramIdNumber, logger);

            if (user == null)
            {
                logger.LogInformation("GetUserByTelegramIdAsync: User not found for TelegramId: {telegramIdNumber}", telegramId);
                return Results.Ok(new UserGetResponse
                {
                    Success = false,
                    Message = "User not found.",
                    ErrorCode = ErrorCodeEnum.ResourceNotFound
                });
            }

            // TODO: create a data mapper
            var userConfiguration = new UserConfigurationDto();
            if (!string.IsNullOrEmpty(user.Configuration.SpreadsheetId))
                userConfiguration.SpreadsheetId = user.Configuration.SpreadsheetId;

            return Results.Ok(new UserGetResponse
                {
                    Success = true,
                    UserId = user.UserId,
                    Message = "User found.",
                    userConfiguration = userConfiguration
                }
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetUserByTelegramIdAsync: Error retrieving user by TelegramId {TelegramId}.",
                telegramIdNumber);
            return Results.InternalServerError("An error occurred while retrieving the user.");
        }
    }
}