using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BudgetAutomation.Engine.AtoTypes;
using BudgetAutomation.Engine.Interface;
using Microsoft.Extensions.Options;
using SharedLibrary.Dto;
using SharedLibrary.Enum;
using SharedLibrary.Model;
using SharedLibrary.Settings;

namespace BudgetAutomation.Engine.ApiClient;

public class UserApiClient : IUserApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserApiClient> _logger;

    public UserApiClient(HttpClient httpClient,
        IOptions<UserApiClientSettings> options,
        ILogger<UserApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(options.Value.Url);
        _httpClient.DefaultRequestHeaders.Add("x-api-key", options.Value.Key);
    }

    public async Task<UserSignupResponse> SignupUserAsync(
        long telegramId, string email, string? username,
        CancellationToken cancellationToken = default)
    {
        var requestUri = new Uri(_httpClient.BaseAddress!, "/user/signup");
        var signupRequest = new UserSignupRequest(telegramId, email, username);

        _logger.LogInformation("Sending signup request for TelegramId {TelegramId} to {RequestUri}", telegramId,
            requestUri);

        try
        {
            // -- TODO: make this into a method (that maybe could live in SharedLibrary)
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            var content = JsonContent.Create(signupRequest, AppJsonSerializerContext.Default.UserSignupRequest);
            request.Content = content;

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to create user for TelegramId {TelegramId}", telegramId);
                return new UserSignupResponse { Success = false };
            }

            var userSignupResponse = await response.Content.ReadFromJsonAsync(
                jsonTypeInfo: AppJsonSerializerContext.Default.UserSignupResponse,
                cancellationToken: cancellationToken);

            if (userSignupResponse is not { Success: true, User.UserId: not null })
            {
                _logger.LogError(
                    "Received success status code but failed to deserialize {ResponseObject} for TelegramId {TelegramId}",
                    typeof(UserSignupResponse), telegramId);
                return new UserSignupResponse { Success = false };
            }

            // --

            _logger.LogInformation("Signup successful for TelegramId {TelegramId}. User ID: {UserId}",
                telegramId, userSignupResponse.User.UserId);
            return userSignupResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed during signup for TelegramId {TelegramId}", telegramId);
            throw; // Re-throw or handle appropriately
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization failed during signup for TelegramId {TelegramId}", telegramId);
            throw; // Re-throw or handle appropriately
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during signup for TelegramId {TelegramId}", telegramId);
            throw; // Re-throw or handle appropriately
        }
    }

    public async Task<UserGetResponse> FindUserByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        var requestUri = new Uri(_httpClient.BaseAddress!, $"/user/telegram/{telegramId}");

        _logger.LogInformation("Sending get request for TelegramId {TelegramId} to {RequestUri}", telegramId,
            requestUri);

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Received status {StatusCode}  when trying to get user for TelegramId {TelegramId}",
                    response.StatusCode, telegramId);
                return new UserGetResponse { Success = false };
            }

            var userGetResponse = await response.Content.ReadFromJsonAsync(
                jsonTypeInfo: AppJsonSerializerContext.Default.UserGetResponse,
                cancellationToken: cancellationToken);

            if (userGetResponse is not { Success: true })
            {
                if (userGetResponse is null)
                {
                    _logger.LogError(
                        "Received success status code but failed to deserialize {ResponseObject} for TelegramId {TelegramId}",
                        typeof(UserGetResponse), telegramId);
                    return new UserGetResponse { Success = false };
                }

                _logger.LogWarning("User not found for TelegramId {TelegramId}", telegramId);
                return new UserGetResponse { Success = false };
            }

            _logger.LogInformation("Signup successful for TelegramId {TelegramId}. User ID: {UserId}",
                telegramId, userGetResponse.UserId);
            return userGetResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed during signup for TelegramId {TelegramId}", telegramId);
            throw; // Re-throw or handle appropriately
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization failed during signup for TelegramId {TelegramId}", telegramId);
            throw; // Re-throw or handle appropriately
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during signup for TelegramId {TelegramId}", telegramId);
            throw; // Re-throw or handle appropriately
        }
    }

    public async Task<bool> UpdateUserConfigurationAsync(
        string userId, UserConfiguration userConfiguration, CancellationToken cancellationToken)
    {
        var requestUri = new Uri(_httpClient.BaseAddress!, $"/user/{userId}/configuration");
        var configurationUpdateRequest = new UserConfigurationUpdateRequest(
            new UserConfigurationDto { SpreadsheetId = userConfiguration.SpreadsheetId }
        );

        _logger.LogInformation("Sending configuration update request for UserId {UserId}", userId);

        try
        {
            // -- TODO: make this into a method (that maybe could live in SharedLibrary)
            var request = new HttpRequestMessage(HttpMethod.Put, requestUri);

            var content = JsonContent.Create(configurationUpdateRequest,
                AppJsonSerializerContext.Default.UserConfigurationUpdateRequest);
            request.Content = content;

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed update user configuration for UserId {UserId}", userId);
                return false;
            }

            var configurationUpdateResponse = await response.Content.ReadFromJsonAsync(
                jsonTypeInfo: AppJsonSerializerContext.Default.UserConfigurationUpdateResponse,
                cancellationToken: cancellationToken);

            if (configurationUpdateResponse is not { Success: true })
            {
                _logger.LogError("Received success status code but failed to deserialize {ResponseObject} for UserId {UserId}",
                    typeof(UserConfigurationUpdateResponse), userId);
                return false;
            }

            // --

            _logger.LogInformation("Configuration update successful for UserId {UserId}", userId);

            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed during signup for UserId {UserId}", userId);
            throw; // Re-throw or handle appropriately
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization failed during signup for UserId {UserId}", userId);
            throw; // Re-throw or handle appropriately
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during signup for UserId {UserId}", userId);
            throw; // Re-throw or handle appropriately
        }
    }
}