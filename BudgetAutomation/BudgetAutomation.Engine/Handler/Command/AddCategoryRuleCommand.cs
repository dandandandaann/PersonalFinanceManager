using BudgetAutomation.Engine.Enums;
using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Misc;
using BudgetAutomation.Engine.Model;
using BudgetAutomation.Engine.Service;
using SharedLibrary.Telegram;

namespace BudgetAutomation.Engine.Handler.Command;

public class AddCategoryRuleCommand(
    ISenderGateway sender,
    ISpreadsheetManagerApiClient spreadsheetManagerApiClient,
    IChatStateService chatStateService)
    : ICommand
{
    public string CommandName => StaticCommandName;
    public static string StaticCommandName => "AddCategoryRule";

    public async Task<Message> HandleAsync(Message message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Text);

        UserManagerService.EnsureUserSignedIn();

        if (string.IsNullOrWhiteSpace(UserManagerService.Configuration.SpreadsheetId))
        {
            return await sender.ReplyAsync(message.Chat,
                $"Por favor configure sua planilha antes de usar o comando /{CommandName}.",
                cancellationToken: cancellationToken);
        }

        if (!Utility.TryExtractCommandArguments(message.Text, CommandName, out string arguments) ||
            string.IsNullOrWhiteSpace(arguments))
        {
            await chatStateService.SetStateAsync(message.Chat.Id, ChatStateEnum.AwaitingArguments, CommandName);
            return await sender.ReplyAsync(message.Chat,
                "Insira a categoria seguida do padrão de descrição. Exemplo: 'Restaurante Lanche'",
                cancellationToken: cancellationToken);
        }

        return await AddCategoryRuleAsync(
            message.Chat,
            arguments,
            cancellationToken);
    }

    public async Task<Message> HandleAsync(Message message, ChatState chatState, CancellationToken cancellationToken = default)
    {
        UserManagerService.EnsureUserSignedIn();

        ArgumentException.ThrowIfNullOrEmpty(message.Text);

        return await AddCategoryRuleAsync(
            message.Chat,
            message.Text,
            cancellationToken);
    }

    private async Task<Message> AddCategoryRuleAsync(Chat chat, string arguments, CancellationToken cancellationToken = default)
    {
        var split = arguments.Split(' ', 2);
        if (split.Length < 2)
        {
            return await sender.ReplyAsync(chat,
                "Formato inválido. Envie a 'Categoria' e também o 'Padrão de Descrição'",
                cancellationToken: cancellationToken);
        }

        var category = split[0];
        category = char.ToUpper(category[0]) + category[1..].ToLower();

        var descriptionPattern = split[1].ToLower();

        var response = await spreadsheetManagerApiClient.AddCategoryRuleAsync(
            UserManagerService.Configuration.SpreadsheetId,
            category,
            descriptionPattern,
            cancellationToken);

        if (!response.Success)
        {
            return await sender.ReplyAsync(chat,
                "Não foi possível criar nova regra de categoria. Tente novamente.",
                cancellationToken: cancellationToken);
        }

        await chatStateService.ClearState(chat.Id);

        return await sender.ReplyAsync(chat,
            "Regra de categoria cadastrada com sucesso!\n" +
            $"Novas despesas sem categoria que incluam '{descriptionPattern}' na descrição serão categorizadas como '{category}'.",
            cancellationToken: cancellationToken);
    }
}