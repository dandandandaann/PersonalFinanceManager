using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Model;
using BudgetAutomation.Engine.Service;
using SharedLibrary.Telegram;

namespace BudgetAutomation.Engine.Handler.Command;

public partial class UndoCommand(
    ISenderGateway sender,
    IExpenseLoggerApiClient expenseLoggerApiClient) : ICommand
{
    public string CommandName => StaticCommandName;
    public static string StaticCommandName => "undo";

    public async Task<Message> HandleAsync(Message message, CancellationToken cancellationToken = default)
    {
        UserManagerService.EnsureUserSignedIn();

        ArgumentNullException.ThrowIfNull(message.Text);

        try
        {
            var spreadsheetId = UserManagerService.Configuration.SpreadsheetId;

            if (string.IsNullOrWhiteSpace(spreadsheetId))
            {
                return await sender.ReplyAsync(message.Chat,
                    "Você ainda não configurou sua planilha. Use o comando /planilha primeiro.",
                    "User attempted to undo without configuring a spreadsheet.",
                    logLevel: LogLevel.Warning,
                    cancellationToken: cancellationToken);

            }

            var response = await expenseLoggerApiClient.RemoveExpenseAsync(UserManagerService.Configuration.SpreadsheetId, cancellationToken);

            if (!response.Success)
            {
                return await sender.ReplyAsync(message.Chat,
                    "Falha ao remover. A planilha pode não existir ou não foi encontrado nenhum item",
                    "User remove failed (not found or API error).",
                    logLevel: LogLevel.Warning,
                    cancellationToken: cancellationToken);
            }
            return await sender.ReplyAsync(message.Chat,
                $"Despesa excluída\n{response.expense}",
                $"Removed expense description {response.expense.Description}",
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString(), ex);
        }
    }

    public Task<Message> HandleAsync(Message message, ChatState chatState, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}