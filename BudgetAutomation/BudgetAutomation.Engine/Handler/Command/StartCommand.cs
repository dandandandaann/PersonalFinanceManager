using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Model;
using BudgetAutomation.Engine.Service;
using SharedLibrary.Telegram;
using SharedLibrary.Telegram.Types.ReplyMarkups;

namespace BudgetAutomation.Engine.Handler.Command;

public class StartCommand(ISenderGateway sender) : ICommand
{
    public string CommandName => StaticCommandName;
    public static string StaticCommandName => "start";

    public async Task<Message> HandleAsync(Message message, CancellationToken cancellationToken = default)
    {
        var startMessage = "Escolha uma das opções";

        var keyboardRows = new List<List<InlineKeyboardButton>>();
        var buttons = new List<InlineKeyboardButton>();

        if (!UserManagerService.UserSignedIn)
        {
            startMessage = "";

            var button = InlineKeyboardButton.WithCallbackData("👤 Cadastrar no sistema", $"/{SignupCommand.StaticCommandName}");

            keyboardRows.Add([button]);
        }
        else if (string.IsNullOrWhiteSpace(UserManagerService.Configuration.SpreadsheetId))
        {
            var button = InlineKeyboardButton.WithCallbackData("⚙️ Configurar Planilha", $"/{SpreadsheetCommand.StaticCommandName}");

            keyboardRows.Add([button]);
        }
        else
        {
            var logButton = InlineKeyboardButton.WithCallbackData("💳 Registrar despesa", $"/{LogCommand.StaticCommandName}");
            var lastItemButton = InlineKeyboardButton.WithCallbackData("🧾 Ver última despesa", $"/{LastItemCommand.StaticCommandName}");
            var undoButton = InlineKeyboardButton.WithCallbackData("🗑️ Deletar última despesa", $"/{UndoCommand.StaticCommandName}");

            keyboardRows.AddRange([[logButton], [lastItemButton], [undoButton]]);
        }

        var inlineKeyboard = new InlineKeyboardMarkup(keyboardRows);

        return await sender.ReplyAsync(
            chat: message.Chat,
            text: startMessage,
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }

    public Task<Message> HandleAsync(Message message, ChatState chatState, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}