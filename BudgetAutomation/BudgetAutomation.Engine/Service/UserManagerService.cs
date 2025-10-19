using System.Globalization;
using BudgetAutomation.Engine.Interface;
using SharedLibrary.Model;

namespace BudgetAutomation.Engine.Service;

public class UserManagerService(IUserApiClient userApiClient) : IUserManagerService
{
    private static readonly AsyncLocal<User> CurrentUser = new();

    private static User? Current
    {
        get => CurrentUser.Value;
        set => CurrentUser.Value = value!;
    }

    public static bool UserSignedIn => !string.IsNullOrWhiteSpace(Current?.UserId);

    public static void EnsureUserSignedIn()
    {
        if (!UserSignedIn)
            throw new UnauthorizedAccessException();
    }


    public static UserConfiguration Configuration => Current?.Configuration ?? new UserConfiguration();

    /// <summary>
    /// Method that authenticates user with UserApi.
    /// This method cannot be asynchronous, because it must run in the same thread as caller to set AsyncLocal CurrentUser.
    /// </summary>
    /// <param name="telegramId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>True if authentication was successful.</returns>
    public bool AuthenticateUser(long telegramId, CancellationToken cancellationToken = default)
    {
        var registeredUser = userApiClient.FindUserByTelegramIdAsync(telegramId, cancellationToken).GetAwaiter().GetResult();

        if (registeredUser.Success == false || registeredUser.UserId == null)
        {
            Current = null;
            return false;
        }

        Current = new User(registeredUser.UserId, telegramId: telegramId);

        if (registeredUser.userConfiguration != null)
        {
            Current.Configuration.SpreadsheetId = registeredUser.userConfiguration.SpreadsheetId;
            Current.Configuration.ExchangeRate = registeredUser.userConfiguration.ExchangeRate;
        }

        return true;
    }

    public bool ConfigureSpreadsheet(string spreadsheetId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Current?.UserId))
            throw new UnauthorizedAccessException();

        Current.Configuration.SpreadsheetId = spreadsheetId;

        return userApiClient.UpdateUserConfigurationAsync(Current.UserId, Current.Configuration, cancellationToken)
            .GetAwaiter().GetResult();
    }

    public string UpdateExchangeRate(string newExchangeRate, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Current?.UserId))
            throw new UnauthorizedAccessException();

        // Use CultureInfo.InvariantCulture for reliable decimal parsing
        if (!double.TryParse(newExchangeRate.Replace(',', '.'), CultureInfo.InvariantCulture, out var doubleAmount)) // This will break if user sends 1,000.00
        {
            throw new ArgumentException("Invalid exchange rate format.", nameof(newExchangeRate));
        }

        Current.Configuration.ExchangeRate = doubleAmount.ToString(CultureInfo.InvariantCulture).Replace(",", "").Replace(".", ",");

        userApiClient.UpdateUserConfigurationAsync(Current.UserId, Current.Configuration, cancellationToken).GetAwaiter().GetResult();

        return Current.Configuration.ExchangeRate;
    }
}