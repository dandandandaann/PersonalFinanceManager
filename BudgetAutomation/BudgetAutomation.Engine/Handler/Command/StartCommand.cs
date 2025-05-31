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
        var startMessage = "Escolha uma das opções:";

        var buttons = new List<InlineKeyboardButton>();

        if (string.IsNullOrWhiteSpace(UserManagerService.Configuration.SpreadsheetId))
            buttons.Add(InlineKeyboardButton.WithCallbackData(text: "⚙️ Planilha", callbackData: SpreadsheetCommand.StaticCommandName));
        else
            buttons.Add(InlineKeyboardButton.WithCallbackData(text: "💳 Registrar Despesa", callbackData: LogCommand.StaticCommandName));

        var inlineKeyboard = new InlineKeyboardMarkup(buttons);

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