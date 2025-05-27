using System.Text.RegularExpressions;
using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Misc;
using BudgetAutomation.Engine.Model;
using BudgetAutomation.Engine.Service;
using SharedLibrary.Enum;
using SharedLibrary.Telegram;

namespace BudgetAutomation.Engine.Handler.Command;

public partial class SpreadsheetCommand(
    ISenderGateway sender,
    IUserManagerService userManagerService,
    IExpenseLoggerApiClient expenseLoggerApiClient) : ICommand
{
    public string CommandName => StaticCommandName;
    // TODO: check if it's possible to have this static property coming from the interface somehow
    public static string StaticCommandName => "planilha";

    public async Task<Message> HandleAsync(Message message, CancellationToken cancellationToken = default)
    {
        UserManagerService.EnsureUserSignedIn();

        ArgumentNullException.ThrowIfNull(message.Text);

        try
        {
            if (!Utility.TryExtractCommandArguments(message.Text, CommandName, SpreadsheetIdRegex, out var spreadsheetId))
            {
                if (string.IsNullOrWhiteSpace(spreadsheetId))
                {
                    return await sender.ReplyAsync(message.Chat,
                        "Por favor envie o ID da sua planilha com esse comando.",
                        "User tried configuring spreadsheet id with empty arguments.",
                        logLevel: LogLevel.Information,
                        cancellationToken: cancellationToken);
                }

                return await sender.ReplyAsync(message.Chat,
                    "ID de planilha inválido, tente novamente.",
                    $"User tried configuring spreadsheet id with bad arguments: '{spreadsheetId}'.",
                    logLevel: LogLevel.Information,
                    cancellationToken: cancellationToken);
            }

            var validationResponse = await expenseLoggerApiClient.ValidateSpreadsheet(spreadsheetId, cancellationToken);

            if (!validationResponse.Success)
            {
                switch (validationResponse.ErrorCode)
                {
                    case ErrorCodeEnum.InvalidInput:
                        return await sender.ReplyAsync(message.Chat,
                            "O ID da planilha é inválido. Verifique o ID e tente novamente.",
                            "Invalid spreadsheet id.",
                            logLevel: LogLevel.Information,
                            cancellationToken: cancellationToken);
                        break;
                    case ErrorCodeEnum.ResourceNotFound:
                        return await sender.ReplyAsync(message.Chat,
                            "Não foi possível encontrar a planilha com o ID enviado. Verifique o ID e tente novamente.",
                            "Spreadsheet not found.",
                            logLevel: LogLevel.Information,
                            cancellationToken: cancellationToken);
                        break;
                    case ErrorCodeEnum.UnknownError:
                    default:
                        return await sender.ReplyAsync(message.Chat,
                            "Ocorreu um erro ao tentar verificar a planilha com o ID informado. Tente novamente mais tarde.",
                            "Unable to validate spreadsheet with provided ID.",
                            logLevel: LogLevel.Information,
                            cancellationToken: cancellationToken);
                }
            }

            var spreadsheetConfigured = userManagerService.ConfigureSpreadsheet(spreadsheetId, cancellationToken);

            if (!spreadsheetConfigured)
            {
                return await sender.ReplyAsync(message.Chat,
                    "Sua planilha é válida, mas não foi possível configurar ela no momento. Por favor tente novamente.",
                    "SpreadsheetId configuration failed.",
                    logLevel: LogLevel.Warning,
                    cancellationToken: cancellationToken);
            }

            return await sender.ReplyAsync(message.Chat,
                "Configuração da planilha realizada com sucesso!",
                "SpreadsheetId configuration successful.",
                cancellationToken: cancellationToken);
        }
        catch (Exception e) when (e is InvalidUserInputException || e is UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception e)
        {

            return await sender.ReplyAsync(message.Chat,
                "Um erro ocorreu ao tentar configurar a planilha. Tente novamente mais tarde.",
                $"Exception during spreadsheet command: {e.Message}",
                logLevel: LogLevel.Error,
                cancellationToken: cancellationToken);
        }
    }

    public Task<Message> HandleAsync(Message message, ChatState chatState, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    [GeneratedRegex("^[a-zA-Z0-9_]+$")]
    private static partial Regex SpreadsheetIdRegex();
}