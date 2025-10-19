using BudgetAutomation.Engine.Enums;
using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Service;
using SharedLibrary.Telegram;
using BudgetAutomation.Engine.Model;
using SharedLibrary.Telegram.Types.ReplyMarkups;

namespace BudgetAutomation.Engine.Handler.Command;

public class ExchangeRateCommand(
    ISenderGateway sender,
    IUserManagerService userManagerService,
    IChatStateService chatStateService) : ICommand
{
    public string CommandName => StaticCommandName;
    public static string StaticCommandName => "ExchangeRate";

    public async Task<Message> HandleAsync(Message message, CancellationToken cancellationToken = default)
    {
        UserManagerService.EnsureUserSignedIn();

        var exchangeRate = UserManagerService.Configuration.ExchangeRate;

        var replyMarkup = new InlineKeyboardMarkup([
            [
                InlineKeyboardButton.WithCallbackData("Atualizar", $"/{UpdateExchangeRateCommand.StaticCommandName}")
            ]
        ]);

        return await sender.ReplyAsync(message.Chat,
            $"A cotação de conversão atual é: {(string.IsNullOrWhiteSpace(exchangeRate) ? "1" : exchangeRate)}",
            "User requested exchange rate.",
            replyMarkup: replyMarkup,
            cancellationToken: cancellationToken);
    }

    public Task<Message> HandleAsync(Message message, ChatState chatState, CancellationToken cancellationToken)
    {
        throw new NotImplementedException($"ExchangeRate state {chatState} not implemented.");
    }
}