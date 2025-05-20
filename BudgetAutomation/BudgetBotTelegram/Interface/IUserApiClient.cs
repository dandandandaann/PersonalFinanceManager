using SharedLibrary.Dto;

namespace BudgetBotTelegram.Interface;

/// <summary>
/// Interface for interacting with the User Manager API.
/// </summary>
public interface IUserApiClient
{
    /// <summary>
    /// Calls the User Manager API to sign up a new user.
    /// </summary>
    /// <param name="telegramId">The user's Telegram ID.</param>
    /// <param name="username">The user's Telegram username (optional).</param>
    /// <param name="email">The user's email (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A response indicating success or failure, potentially including user details.</returns>
    Task<UserSignupResponse> SignupUserAsync(long telegramId, string? username, string? email, CancellationToken cancellationToken = default);

    Task<UserGetResponse> FindUserByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default);
}