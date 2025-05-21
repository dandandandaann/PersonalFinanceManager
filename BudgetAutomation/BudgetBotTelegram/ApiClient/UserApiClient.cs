using System.Net;
using System.Text.Json;
using BudgetBotTelegram.AtoTypes;
using BudgetBotTelegram.Interface;
using Microsoft.Extensions.Options;
using SharedLibrary.Dto;
using SharedLibrary.Model;
using SharedLibrary.Settings;

namespace BudgetBotTelegram.ApiClient;

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
        long telegramId, string? username, string? email,
        CancellationToken cancellationToken = default)
    {
        var requestUri = new Uri(_httpClient.BaseAddress!, "/user/signup");
        var signupRequest = new UserSignupRequest(telegramId, username);

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

            var userResponse = await response.Content.ReadFromJsonAsync(
                jsonTypeInfo: AppJsonSerializerContext.Default.UserSignupResponse,
                cancellationToken: cancellationToken);

            if (userResponse is not { Success: true, User: not null })
            {
                _logger.LogError(
                    "Received success status code but failed to deserialize UserResponse for TelegramId {TelegramId}",
                    telegramId);
                return new UserSignupResponse { Success = false };
            }

            // --

            _logger.LogInformation("Signup successful for TelegramId {TelegramId}. User ID: {UserId}",
                telegramId, userResponse.User.UserId);
            return userResponse;
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

        _logger.LogInformation("Sending signup request for TelegramId {TelegramId} to {RequestUri}", telegramId,
            requestUri);

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    _logger.LogError("Failed to check user for TelegramId {TelegramId}", telegramId);
                return new UserGetResponse { Success = false };
            }

            var userGetResponse = await response.Content.ReadFromJsonAsync(
                jsonTypeInfo: AppJsonSerializerContext.Default.UserGetResponse,
                cancellationToken: cancellationToken);

            if (userGetResponse is not { Success: true })
            {
                _logger.LogError(
                    "Received success status code but failed to deserialize {ResponseObject} for TelegramId {TelegramId}",
                    typeof(UserGetResponse), telegramId);
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

        _logger.LogInformation("Sending Configuration Update request for UserId {UserId}", userId);

        try
        {
            // -- TODO: make this into a method (that maybe could live in SharedLibrary)
            var request = new HttpRequestMessage(HttpMethod.Put, requestUri);

            var content = JsonContent.Create(configurationUpdateRequest, AppJsonSerializerContext.Default.UserConfigurationUpdateRequest);
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
                _logger.LogError(
                    "Received success status code but failed to deserialize UserResponse for UserId {UserId}",
                    userId);
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