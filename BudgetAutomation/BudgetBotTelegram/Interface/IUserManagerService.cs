namespace BudgetBotTelegram.Interface;

public interface IUserManagerService
{
    bool AuthenticateUser(long telegramId, CancellationToken cancellationToken = default);

    bool ConfigureSpreadsheet(string spreadsheetId, CancellationToken cancellationToken);
}