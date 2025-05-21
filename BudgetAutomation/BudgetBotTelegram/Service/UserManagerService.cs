using BudgetBotTelegram.Interface;
using SharedLibrary.Model;

namespace BudgetBotTelegram.Service;

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
            Current.Configuration.SpreadsheetId = registeredUser.userConfiguration.SpreadsheetId;

        return true;
    }

    public bool ConfigureSpreadsheet(string spreadsheetId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Current?.UserId))
            throw new UnauthorizedAccessException();

        Current.Configuration ??= new UserConfiguration();

        Current.Configuration.SpreadsheetId = spreadsheetId;

        return userApiClient.UpdateUserConfigurationAsync(Current.UserId, Current.Configuration, cancellationToken)
            .GetAwaiter().GetResult();
    }
}