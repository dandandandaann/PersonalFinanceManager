using BudgetAutomation.Engine.Enums;
using BudgetAutomation.Engine.Handler.Command.Alias;
using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Misc;
using BudgetAutomation.Engine.Model;
using BudgetAutomation.Engine.Service;
using SharedLibrary.Constants;
using SharedLibrary.Enum;
using SharedLibrary.Telegram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetAutomation.Engine.Handler.Command
{
    public class ReturnSpreadsheetCommand(
        ISenderGateway sender,
        IUserManagerService userManagerService,
        ISpreadsheetManagerApiClient spreadsheetManagerApiClient,
        IChatStateService chatStateService) : ICommand
    {
        public string CommandName => StaticCommandName;

        public static string StaticCommandName => "spreadsheeturl";

        public async Task<Message> HandleAsync(Message message, CancellationToken cancellationToken = default)
        {
            UserManagerService.EnsureUserSignedIn();

            var chat = message.Chat;

            var spreadsheetId = UserManagerService.Configuration.SpreadsheetId;

            if (string.IsNullOrWhiteSpace(spreadsheetId))
            {
                return await sender.ReplyAsync(chat,
                "Você ainda não configurou uma planilha.\n" +
                $"Use o comando /{SpreadsheetCommand.StaticCommandName} para configurar uma.",
                "User requested spreadsheet info without configuring one.",
                cancellationToken: cancellationToken);
            }

            var spreadsheetUrl = SpreadsheetConstants.Urls.spreadsheetUrl +spreadsheetId;

            ArgumentNullException.ThrowIfNull(message.Text);

            return await sender.ReplyAsync(chat,
               $"Sua planilha configurada é:\n{spreadsheetUrl}",
               "Spreadsheet info returned successfully.",
               cancellationToken: cancellationToken);
        }

    public Task<Message> HandleAsync(Message message, ChatState chatState, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
