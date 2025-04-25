using System.Net;
using System.Text.Json;
using BudgetBotTelegram.AtoTypes;
using BudgetBotTelegram.Interface;
using SharedLibrary.UserClasses;

namespace BudgetBotTelegram.ApiClient;

public class UserApiClient(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<UserApiClient> logger) : IUserApiClient
{
    private readonly Uri _userManagerUri = new(
        configuration["UserManagerApiUrl"] ??
        throw new InvalidOperationException("UserManagerApiUrl is not configured.")
    );

    public async Task<UserResponse> SignupUserAsync(long telegramId, string? username,
        CancellationToken cancellationToken)
    {
        var requestUri = new Uri(_userManagerUri, "/user/signup");
        var signupRequest = new UserSignupRequest(telegramId, username);

        logger.LogInformation("Sending signup request for TelegramId {TelegramId} to {RequestUri}", telegramId,
            requestUri);

        try
        {
            // -- TODO: make this into a method (that maybe could live in SharedLibrary)
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            var content = JsonContent.Create(signupRequest, AppJsonSerializerContext.Default.UserSignupRequest);
            request.Content = content;

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to create user for TelegramId {TelegramId}", telegramId);
                return new UserResponse { Success = false };
            }

            var userResponse = await response.Content.ReadFromJsonAsync(
                jsonTypeInfo: AppJsonSerializerContext.Default.UserResponse,
                cancellationToken: cancellationToken);

            if (userResponse is not { Success: true, User: not null })
            {
                logger.LogError(
                    "Received success status code but failed to deserialize UserResponse for TelegramId {TelegramId}",
                    telegramId);
                return new UserResponse { Success = false };
            }

            // --

            logger.LogInformation("Signup successful for TelegramId {TelegramId}. User ID: {UserId}",
                telegramId, userResponse.User.UserId);
            return userResponse;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP request failed during signup for TelegramId {TelegramId}", telegramId);
            throw; // Re-throw or handle appropriately
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "JSON deserialization failed during signup for TelegramId {TelegramId}", telegramId);
            throw; // Re-throw or handle appropriately
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred during signup for TelegramId {TelegramId}", telegramId);
            throw; // Re-throw or handle appropriately
        }
    }

    public async Task<UserExistsResponse> CheckUserAsync(long telegramId, CancellationToken cancellationToken)
    {
        var requestUri = new Uri(_userManagerUri, $"/user/telegram/{telegramId}");

        logger.LogInformation("Sending signup request for TelegramId {TelegramId} to {RequestUri}", telegramId,
            requestUri);

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    logger.LogError("Failed to check user for TelegramId {TelegramId}", telegramId);
                return new UserExistsResponse { Success = false };
            }

            var userExistsResponse = await response.Content.ReadFromJsonAsync(
                jsonTypeInfo: AppJsonSerializerContext.Default.UserExistsResponse,
                cancellationToken: cancellationToken);

            if (userExistsResponse is not { Success: true })
            {
                logger.LogError(
                    "Received success status code but failed to deserialize UserExistsResponse for TelegramId {TelegramId}",
                    telegramId);
                return new UserExistsResponse { Success = false };
            }

            logger.LogInformation("Signup successful for TelegramId {TelegramId}. User ID: {UserId}",
                telegramId, userExistsResponse.UserId);
            return userExistsResponse;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP request failed during signup for TelegramId {TelegramId}", telegramId);
            throw; // Re-throw or handle appropriately
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "JSON deserialization failed during signup for TelegramId {TelegramId}", telegramId);
            throw; // Re-throw or handle appropriately
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred during signup for TelegramId {TelegramId}", telegramId);
            throw; // Re-throw or handle appropriately
        }
    }
}