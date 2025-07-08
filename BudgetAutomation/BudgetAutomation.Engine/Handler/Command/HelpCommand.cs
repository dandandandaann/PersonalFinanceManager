using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Misc;
using BudgetAutomation.Engine.Model;
using BudgetAutomation.Engine.Service;
using SharedLibrary.Constants;
using SharedLibrary.Telegram;
using SharedLibrary.Telegram.Enums;
using SharedLibrary.Telegram.Types.ReplyMarkups;
using System.Text;
using BudgetAutomation.Engine.Handler.Command.Alias;

namespace BudgetAutomation.Engine.Handler.Command;

public class HelpCommand(ISenderGateway sender) : ICommand
{
    public string CommandName => StaticCommandName;
    public static string StaticCommandName => "help";

    private enum HelpArgumentEnum
    {
        SpreadsheetTemplateUrl,
        SpreadsheetCopy,
        SpreadsheetShare,
        SpreadsheetConfiguration,
        Commands,
        About,
        Contact,
        ReportProblem,
    }

    public async Task<Message> HandleAsync(Message message, CancellationToken cancellationToken = default)
    {
        ParseMode parseMode = ParseMode.None;
        var helpMessage = new StringBuilder("Escolha uma das opções de ajuda");

        if (!Utility.TryExtractCommandArguments(message.Text, CommandName, out var arguments) ||
            string.IsNullOrWhiteSpace(arguments) ||
            !Enum.TryParse<HelpArgumentEnum>(arguments.Replace(" ", string.Empty), true, out var helpArg))
        {
            var keyboardRows = new List<List<InlineKeyboardButton>>();

            var commandsButton = Utility.Button("Comandos", CommandName, HelpArgumentEnum.Commands);
            keyboardRows.Add([commandsButton]);

            if (!UserManagerService.UserSignedIn)
            {
                // TODO: add more help options
                // var button = Utility.Button("❓ Ajuda", $"/{HelpCommand.StaticCommandName}");
                //
                // keyboardRows.Add([button]);
            }
            else if (string.IsNullOrWhiteSpace(UserManagerService.Configuration.SpreadsheetId))
            {
                var spreadsheetTemplateUrl = Utility.Button("Link do template", CommandName,
                    HelpArgumentEnum.SpreadsheetTemplateUrl);
                var spreadsheetCopy = Utility.Button("Copiar a planilha", CommandName,
                    HelpArgumentEnum.SpreadsheetCopy);
                var spreadsheetShare = Utility.Button("Compartilhar a planilha", CommandName,
                    HelpArgumentEnum.SpreadsheetShare);

                keyboardRows.AddRange([[spreadsheetTemplateUrl], [spreadsheetCopy], [spreadsheetShare]]);
            }
            else
            {
                var reportProblemButton = Utility.Button("Reportar um problema", CommandName, HelpArgumentEnum.ReportProblem);

                keyboardRows.Add([reportProblemButton]);
            }

            var aboutButton = Utility.Button("Sobre", CommandName, HelpArgumentEnum.About);
            var contactButton = Utility.Button("Contato", CommandName, HelpArgumentEnum.Contact);

            keyboardRows.Add([contactButton, aboutButton]);

            var inlineKeyboard = new InlineKeyboardMarkup(keyboardRows);

            return await sender.ReplyAsync(
                chat: message.Chat,
                text: helpMessage.ToString(),
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        helpMessage.Clear();

        switch (helpArg)
        {
            case HelpArgumentEnum.SpreadsheetCopy:
                parseMode = ParseMode.Html;
                helpMessage.AppendLine("<b>Copiar a planilha:</b>");
                helpMessage.AppendLine();

                helpMessage.AppendLine("Você precisa ter sua própria planilha salva no Google Drive.");
                helpMessage.AppendLine($"O {BotConstants.Name} te ajuda a registrar novas despesas, mas você vai também " +
                                       $"vai poder acessar sua planilha para gerenciar suas despesas diretamente.");
                helpMessage.AppendLine();
                helpMessage.AppendLine("Você não pode usar qualquer planilha, precisa copiar o template " +
                                       $"porque {BotConstants.Name} só sabe registrar despesas nessa planilha específica.");
                helpMessage.AppendLine();
                helpMessage.AppendLine("Para copiar a planilha você precisa acessar a planilha template, " +
                                       "clicar no menu de opções a escolher a opção \"Fazer uma Cópia\".");
                break;
            case HelpArgumentEnum.SpreadsheetTemplateUrl:
                helpMessage.AppendLine("Este é o link da planilha de template: ");
                helpMessage.AppendLine(SpreadsheetConstants.TemplateUrl);
                break;
            case HelpArgumentEnum.SpreadsheetShare:
                parseMode = ParseMode.Html;
                helpMessage.AppendLine("<b>Compartilhar a planilha:</b>");
                helpMessage.AppendLine();

                helpMessage.AppendLine("Após copiar a planilha, você ainda precisa compartilhar ela.");
                helpMessage.AppendLine($"Igual você pode dar acesso a outras pessoas, você precisa dar acesso ao {BotConstants.Name} " +
                                       "para que ele também possa escrever nela.");
                helpMessage.AppendLine();
                helpMessage.AppendLine($"Para compartilhar sua planilha com o <b>{BotConstants.Name}</b> você precisa:");
                helpMessage.AppendLine("<b>1.</b> Acessar a planilha que você criou a cópia");
                helpMessage.AppendLine("<b>2.</b> Abrir o menu de opções");
                helpMessage.AppendLine("<b>3.</b> Entrar em \"<b>Compartilhar e exportar</b>\"");
                helpMessage.AppendLine("<b>4.</b> Escolher a opção \"<b>Compartilhar</b>\"");
                helpMessage.AppendLine($"<b>5.</b> Adicionar o email: \"<i>{BotConstants.Email}</i>\".");
                helpMessage.AppendLine();
                helpMessage.AppendLine($"Depois disso você precisa usar o comando /{PlanilhaCommandAlias.StaticCommandName} para " +
                                       $"enviar o link da sua planilha para o {BotConstants.Name}.");
                break;
            case HelpArgumentEnum.SpreadsheetConfiguration:
                helpMessage.Append("TODO: essa opção deveria mandar as opções de configuração de planilha.");
                break;
            case HelpArgumentEnum.Commands:
                parseMode = ParseMode.Html;
                helpMessage.AppendLine("<b>Comandos:</b>");
                helpMessage.AppendLine();
                helpMessage.AppendLine($"Comandos no Telegram são mensagens que começam com /, " +
                                       $"como por exemplo o /{RegistrarCommandAlias.StaticCommandName}. " +
                                       $"Eles são usados para enviar instruções específicas para bots como o {BotConstants.Name}.");
                helpMessage.AppendLine("Você pode executar comandos de 3 maneiras:");
                helpMessage.AppendLine("<b>•</b> Escrevendo o comando diretamente no chat");
                helpMessage.AppendLine($"<b>•</b> Clicando no comando que já está escrito -> /{AjudaCommandAlias.StaticCommandName}");
                helpMessage.AppendLine("<b>•</b> Clicando em algum botão das mensagens que o Bot te enviou");
                break;
            case HelpArgumentEnum.About:
                helpMessage.AppendLine("Este sistema ajuda você a gerenciar suas finanças pessoais de forma simples e integrada ao Google Sheets.");
                helpMessage.AppendLine($"Desenvolvido por {BotConstants.Creator}.");
                break;
            case HelpArgumentEnum.Contact:
                helpMessage.AppendLine($"Contato do Telegram: {BotConstants.Creator}.");
                break;
            case HelpArgumentEnum.ReportProblem:
                helpMessage.AppendLine($"Se você encontrou algum problema, por favor, entre em contato pelo Telegram e descreva o problema encontrado: {BotConstants.Creator}.");
                break;
            default:
                throw new NotImplementedException($"{nameof(HelpArgumentEnum)} option {helpArg} not implemented.");
        }

        return await sender.ReplyAsync(message.Chat,
            helpMessage.ToString(),
            parseMode: parseMode,
            cancellationToken: cancellationToken);
    }

    public Task<Message> HandleAsync(Message message, ChatState chatState, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}