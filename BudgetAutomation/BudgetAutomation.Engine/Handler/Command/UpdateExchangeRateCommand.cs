using BudgetAutomation.Engine.Enums;
using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Service;
using SharedLibrary.Telegram;
using System.Globalization;
using BudgetAutomation.Engine.Model;

namespace BudgetAutomation.Engine.Handler.Command;

public class UpdateExchangeRateCommand(
    ISenderGateway sender,
    IUserManagerService userManagerService,
    IChatStateService chatStateService) : ICommand
{
    public string CommandName => StaticCommandName;
    public static string StaticCommandName => "UpdateExchangeRate";

    public async Task<Message> HandleAsync(Message message, CancellationToken cancellationToken = default)
    {
        UserManagerService.EnsureUserSignedIn();

        var exchangeRate = UserManagerService.Configuration.ExchangeRate;

        await chatStateService.SetStateAsync(message.Chat.Id, ChatStateEnum.AwaitingArguments, CommandName);
        return await sender.ReplyAsync(message.Chat,
            $"Digite o novo valor cotação usada para calcular o total de cada despesa.",
            "Request new exchange rate.",
            cancellationToken: cancellationToken);
    }

    public async Task<Message> HandleAsync(Message message, ChatState chatState, CancellationToken cancellationToken)
    {
        UserManagerService.EnsureUserSignedIn();
        ArgumentException.ThrowIfNullOrEmpty(message.Text);

        if (chatState.State == nameof(ChatStateEnum.AwaitingArguments) && chatState.ActiveCommand == StaticCommandName)
        {
            if (double.TryParse(message.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                var exchangeRate = userManagerService.UpdateExchangeRate(message.Text, cancellationToken);
                await chatStateService.ClearState(message.Chat.Id);
                return await sender.ReplyAsync(message.Chat,
                    $"Cotação de conversão atualizada para: {exchangeRate}",
                    "User updated exchange rate.",
                    cancellationToken: cancellationToken);
            }

            return await sender.ReplyAsync(message.Chat,
                "Valor de conversão inválido. Por favor envie um número válido.",
                "User sent invalid exchange rate value.",
                logLevel: LogLevel.Warning,
                cancellationToken: cancellationToken);
        }

        throw new NotImplementedException($"ExchangeRate state {chatState} not implemented.");
    }
}