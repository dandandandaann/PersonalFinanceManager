using System.Text;
using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Model;
using BudgetAutomation.Engine.Service;
using SharedLibrary.Telegram;
using SharedLibrary.Telegram.Types.ReplyMarkups;

namespace BudgetAutomation.Engine.Handler.Command;

public class CategoryCommand(ISenderGateway sender) : ICommand
{
    public string CommandName => StaticCommandName;
    public static string StaticCommandName => "category";

    public async Task<Message> HandleAsync(Message message, CancellationToken cancellationToken = default)
    {
        var startMessage = new StringBuilder("Escolha uma das opções");

        var keyboardRows = new List<List<InlineKeyboardButton>>();

        var addCategoryRuleButton = InlineKeyboardButton.WithCallbackData("🔁 Criar regra de categoria", $"/{AddCategoryRuleCommand.StaticCommandName}");
        var listCategoriesButton = InlineKeyboardButton.WithCallbackData("🗃 Listar categorias", $"/{ListCategoriesCommand.StaticCommandName}");

        keyboardRows.AddRange([[addCategoryRuleButton], [listCategoriesButton]]);

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