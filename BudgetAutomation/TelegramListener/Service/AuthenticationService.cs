using Microsoft.Extensions.Options;
using SharedLibrary.Settings;

namespace TelegramListener.Service;

public interface IAuthenticationService
{
    bool IsAuthorized(string providedToken);
}

public class AuthenticationService(IOptions<TelegramBotSettings> telegramBotOptions) : IAuthenticationService
{
    private readonly TelegramBotSettings _botSettings = telegramBotOptions.Value;

    public bool IsAuthorized(string providedToken)
    {
        return providedToken == _botSettings.WebhookToken;
    }
}