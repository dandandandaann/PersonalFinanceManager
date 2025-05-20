using BudgetBotTelegram.Interface;
using SharedLibrary.Model;

namespace BudgetBotTelegram.Service;

public class UserManagerService(IUserApiClient userApiClient) : IUserManagerService
{
    private static readonly AsyncLocal<User> CurrentUser = new();
    private static readonly AsyncLocal<UserConfiguration> UserConfiguration = new();

    private static User? Current
    {
        get => CurrentUser.Value;
        set => CurrentUser.Value = value!;
    }
    public static bool UserLoggedIn => !string.IsNullOrWhiteSpace(Current?.UserId);

    public static UserConfiguration Configuration
    {
        get => UserConfiguration.Value!;
        set => UserConfiguration.Value = value;
    }

    public bool AuthenticateUser(long telegramId, CancellationToken cancellationToken = default)
    {
        var registeredUser = userApiClient.FindUserByTelegramIdAsync(telegramId, cancellationToken).GetAwaiter().GetResult();

        if (registeredUser.Success == false || registeredUser.UserId == null)
        {
            Current = null;
            return false;
        }

        Current = new(registeredUser.UserId, telegramId: telegramId);
        Configuration = new UserConfiguration
        {
            SpreadsheetId = registeredUser.userConfiguration.SpreadsheetId,
        };

        return true;

    }
}