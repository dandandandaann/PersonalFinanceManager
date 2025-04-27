using BudgetBotTelegram.Interface;
using SharedLibrary.UserClasses;

namespace BudgetBotTelegram.Service;

public class UserManagerService(IUserApiClient userApiClient) : IUserManagerService
{
    private static readonly AsyncLocal<User> CurrentUser = new();

    public static User Current
    {
        get => CurrentUser.Value!;
        set => CurrentUser.Value = value;
    }
    public static bool UserLoggedIn => !string.IsNullOrWhiteSpace(CurrentUser.Value?.UserId);

    public bool AuthenticateUser(long telegramId, CancellationToken cancellationToken = default)
    {
        var registeredUser = userApiClient.CheckUserAsync(telegramId, cancellationToken).GetAwaiter().GetResult();

        if (registeredUser.Success == false || registeredUser.UserId == null)
        {
            return false;
        }

        Current = new(registeredUser.UserId, telegramId: telegramId);

        return true;

    }
}