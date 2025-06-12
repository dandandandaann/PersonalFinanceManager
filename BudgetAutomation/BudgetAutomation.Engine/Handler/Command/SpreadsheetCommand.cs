using System.Text.RegularExpressions;
using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Misc;
using BudgetAutomation.Engine.Model;
using BudgetAutomation.Engine.Service;
using SharedLibrary.Constants;
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
    public static string StaticCommandName => "spreadsheet";

    public async Task<Message> HandleAsync(Message message, CancellationToken cancellationToken = default)
    {
        UserManagerService.EnsureUserSignedIn();

        ArgumentNullException.ThrowIfNull(message.Text);

        try
        {
            Utility.TryExtractCommandArguments(message.Text, CommandName, null, out var arguments);

            if (string.IsNullOrWhiteSpace(arguments))
            {
                return await sender.ReplyAsync(message.Chat,
                    "Por favor envie o ID ou o endereço da sua planilha com esse comando.",
                    "User tried configuring spreadsheet id with empty arguments.",
                    logLevel: LogLevel.Information,
                    cancellationToken: cancellationToken);
            }

            var spreadsheetId = ExtractSpreadsheetIdFromInput(arguments);

            if (spreadsheetId is null || !SpreadsheetIdRegex().IsMatch(spreadsheetId))
            {
                return await sender.ReplyAsync(message.Chat,
                    "Planilha inválida.\n" +
                    "Verifique o endereço enviado e tente novamente.",
                    $"User tried configuring spreadsheet id with bad arguments: '{arguments}'.",
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
                            "O endereço da planilha é inválido.\n" +
                            "Verifique o endereço enviado e tente novamente.",
                            "Invalid spreadsheet id.",
                            logLevel: LogLevel.Information,
                            cancellationToken: cancellationToken);
                    case ErrorCodeEnum.ResourceNotFound:
                        return await sender.ReplyAsync(message.Chat,
                            "Não foi possível encontrar a planilha.\n" +
                            "Verifique o endereço enviado e tente novamente.",
                            "Spreadsheet not found.",
                            logLevel: LogLevel.Information,
                            cancellationToken: cancellationToken);
                    case ErrorCodeEnum.TransactionsSheetNotFound:
                        return await sender.ReplyAsync(message.Chat,
                            $"Falha ao configurar a planilha.\n" +
                            $"A planilha enviada não contém a aba de {SpreadsheetConstants.Sheets.Transactions}.",
                            "Spreadsheet not found.",
                            logLevel: LogLevel.Information,
                            cancellationToken: cancellationToken);
                    case ErrorCodeEnum.UnauthorizedAccess:
                        return await sender.ReplyAsync(message.Chat,
                            "O sistema não tem permissão para acessar planilha enviada.\n" +
                            "Verifique se a planilha está compartilhada corretamente.",
                            logLevel: LogLevel.Information,
                            cancellationToken: cancellationToken);
                    case ErrorCodeEnum.UnknownError:
                    default:
                        return await sender.ReplyAsync(message.Chat,
                            "Ocorreu um erro ao tentar verificar a planilha com o ID informado.\n Tente novamente mais tarde.",
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
                "Configuração da planilha realizada com sucesso!\n" +
                $"Acione o comando /{StartCommand.StaticCommandName} para ver todas as opções.",
                "SpreadsheetId configuration successful.",
                cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (ex is InvalidUserInputException || ex is UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {

            return await sender.ReplyAsync(message.Chat,
                "Um erro ocorreu ao tentar configurar a planilha. Tente novamente mais tarde.",
                $"Exception during spreadsheet command: {ex.Message}",
                logLevel: LogLevel.Error,
                cancellationToken: cancellationToken);
        }
    }

    public Task<Message> HandleAsync(Message message, ChatState chatState, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private static string? ExtractSpreadsheetIdFromInput(string input)
    {
        var trimmedInput = input.Trim();
        var match = SpreadsheetUrlRegex().Match(trimmedInput);

        if (match.Success)
            return match.Groups[1].Value;

        return trimmedInput;
    }

    [GeneratedRegex(@"/spreadsheets/d/([a-zA-Z0-9_-]{40,})")]
    private static partial Regex SpreadsheetUrlRegex();

    [GeneratedRegex("^[a-zA-Z0-9_-]{40,}$")]
    private static partial Regex SpreadsheetIdRegex();
}