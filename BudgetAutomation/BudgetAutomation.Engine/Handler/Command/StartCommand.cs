using System.Text;
using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Misc;
using BudgetAutomation.Engine.Model;
using BudgetAutomation.Engine.Service;
using SharedLibrary.Constants;
using SharedLibrary.Telegram;
using SharedLibrary.Telegram.Types.ReplyMarkups;

namespace BudgetAutomation.Engine.Handler.Command;

public class StartCommand(ISenderGateway sender) : ICommand
{
    public string CommandName => StaticCommandName;
    public static string StaticCommandName => "start";

    public async Task<Message> HandleAsync(Message message, CancellationToken cancellationToken = default)
    {
        var startMessage = new StringBuilder("Escolha uma das opções");

        var keyboardRows = new List<List<InlineKeyboardButton>>();
        var buttons = new List<InlineKeyboardButton>();

        if (!UserManagerService.UserSignedIn)
        {
            startMessage.Clear();
            startMessage.AppendLine($"{Utility.GetGreetingByTimeOfDay()}!");
            startMessage.AppendLine($"Meu chamo {BotConstants.Name} e vou te ajudar a registrar suas despesas.");
            startMessage.AppendLine("Clique no botão abaixo e siga os passos de cadastro:");

            var button = InlineKeyboardButton.WithCallbackData("👤 Cadastrar no sistema", $"/{SignupCommand.StaticCommandName}");

            keyboardRows.Add([button]);
        }
        else if (string.IsNullOrWhiteSpace(UserManagerService.Configuration.SpreadsheetId))
        {
            startMessage.Clear();
            startMessage.AppendLine("Agora você precisa configurar sua planilha.");
            startMessage.AppendLine("Para que eu consigo registrar suas despesas você precisa " +
                                    "copiar a planilha de template e compartilhar ela comigo.");
            startMessage.AppendLine();
            startMessage.AppendLine($"Esse é o meu email: {BotConstants.Email}");
            startMessage.AppendLine($"Esse é o link da planilha de template: {SpreadsheetConstants.TemplateUrl}");
            startMessage.AppendLine();
            startMessage.AppendLine("Clique no botão abaixo para continuar ou em ajuda para entender melhor como fazer isso:");

            var button = InlineKeyboardButton.WithCallbackData("⚙️ Configurar Planilha",
                $"/{SpreadsheetCommand.StaticCommandName}");

            keyboardRows.Add([button]);
        }
        else
        {
            var logButton = InlineKeyboardButton.WithCallbackData("💳 Registrar despesa", $"/{LogCommand.StaticCommandName}");
            var lastItemButton =
                InlineKeyboardButton.WithCallbackData("🧾 Ver última despesa", $"/{LastItemCommand.StaticCommandName}");
            var undoButton =
                InlineKeyboardButton.WithCallbackData("🗑️ Deletar última despesa", $"/{UndoCommand.StaticCommandName}");

            keyboardRows.AddRange([[logButton], [lastItemButton], [undoButton]]);
        }

        var helpButton = InlineKeyboardButton.WithCallbackData("❓ Ajuda", $"/{HelpCommand.StaticCommandName}");
        keyboardRows.Add([helpButton]);

        var inlineKeyboard = new InlineKeyboardMarkup(keyboardRows);

        return await sender.ReplyAsync(
            chat: message.Chat,
            text: startMessage.ToString(),
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }

    public Task<Message> HandleAsync(Message message, ChatState chatState, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}