using BudgetAutomation.Engine.Handler.Command.Alias;
using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Model;
using BudgetAutomation.Engine.Service;
using SharedLibrary.Enum;
using SharedLibrary.Telegram;

namespace BudgetAutomation.Engine.Handler.Command;

public class LastItemCommand(
    ISenderGateway sender,
    ISpreadsheetManagerApiClient SpreadsheetManagerApiClient) : ICommand
{
    public string CommandName => StaticCommandName;
    public static string StaticCommandName => "lastitem";

    public async Task<Message> HandleAsync(Message message, CancellationToken cancellationToken = default)
    {
        UserManagerService.EnsureUserSignedIn();

        if (string.IsNullOrWhiteSpace(UserManagerService.Configuration.SpreadsheetId))
        {
            return await sender.ReplyAsync(message.Chat,
                $"Por favor configure sua planilha com o commando /{PlanilhaCommandAlias.StaticCommandName} " +
                $"antes de usar o comando /{CommandName}.",
                cancellationToken: cancellationToken);
        }

        var chat = message.Chat;

        ArgumentNullException.ThrowIfNull(message.Text);

        var spreadsheetId = UserManagerService.Configuration.SpreadsheetId;

        if (string.IsNullOrWhiteSpace(spreadsheetId))
        {
            return await sender.ReplyAsync(chat,
                "Você ainda não configurou sua planilha. Use o comando /planilha primeiro.",
                "User attempted to undo without configuring a spreadsheet.",
                logLevel: LogLevel.Warning,
                cancellationToken: cancellationToken);
        }

        var response =
            await SpreadsheetManagerApiClient.GetLastExpenseAsync(UserManagerService.Configuration.SpreadsheetId, cancellationToken);

        if (!response.Success)
        {
            switch (response.ErrorCode)
            {
                case ErrorCodeEnum.ResourceNotFound:
                    return await sender.ReplyAsync(chat,
                        "O comando falhou porque a planilha ou uma das abas necessárias não existem." +
                        "Tente configurar a planilha novamente.",
                        "User log failed spreadsheet or sheet was not found.",
                        logLevel: LogLevel.Warning,
                        cancellationToken: cancellationToken);
                case ErrorCodeEnum.UnauthorizedAccess:
                    return await sender.ReplyAsync(chat,
                        "O comando falhou porque o sistema não tem permissão na planilha. " +
                        "Verifique se a planilha está compartilhada corretamente.",
                        "User log failed due to unauthorized access.",
                        logLevel: LogLevel.Warning,
                        cancellationToken: cancellationToken);
                default:
                    return await sender.ReplyAsync(chat,
                        "O comando falhou. Tente novamente.",
                        "User log failed due to API error.",
                        logLevel: LogLevel.Warning,
                        cancellationToken: cancellationToken);
            }
        }

        return await sender.ReplyAsync(chat,
            $"Última despesa registrada\n{response.expense}",
            cancellationToken: cancellationToken);
    }

    public Task<Message> HandleAsync(Message message, ChatState chatState, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}