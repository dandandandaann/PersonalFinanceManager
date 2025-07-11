﻿using System.Text.RegularExpressions;
using BudgetAutomation.Engine.Enums;
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
    ISpreadsheetManagerApiClient spreadsheetManagerApiClient,
    IChatStateService chatStateService) : ICommand
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
            Utility.TryExtractCommandArguments(message.Text, CommandName, out var arguments);

            if (string.IsNullOrWhiteSpace(arguments))
            {
                await chatStateService.SetStateAsync(message.Chat.Id, ChatStateEnum.AwaitingArguments, CommandName);

                return await sender.ReplyAsync(message.Chat,
                    "Por favor compartilhe o link da sua planilha.",
                    "User called spreadsheet command without arguments.",
                    logLevel: LogLevel.Information,
                    cancellationToken: cancellationToken);
            }

            return await ConfigureSpreadsheetAsync(message.Chat, arguments, cancellationToken);
        }
        catch (Exception ex) when (ex is InvalidUserInputException or UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return await sender.ReplyAsync(message.Chat,
                "Ocorreu um erro ao tentar configurar a planilha. Tente novamente mais tarde.",
                $"Exception during spreadsheet command: {ex}",
                logLevel: LogLevel.Error,
                cancellationToken: cancellationToken);
        }
    }

    public async Task<Message> HandleAsync(Message message, ChatState chatState, CancellationToken cancellationToken)
    {
        try
        {
            UserManagerService.EnsureUserSignedIn();

            ArgumentException.ThrowIfNullOrEmpty(message.Text);

            if (chatState.State == ChatStateEnum.AwaitingArguments.ToString())
            {
                return await ConfigureSpreadsheetAsync(message.Chat, message.Text, cancellationToken);
            }
        }
        catch (Exception ex) when (ex is InvalidUserInputException or UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return await sender.ReplyAsync(message.Chat,
                "Ocorreu um erro ao tentar configurar a planilha. Tente novamente mais tarde.",
                $"Exception during spreadsheet command: {ex}",
                logLevel: LogLevel.Error,
                cancellationToken: cancellationToken);
        }

        throw new NotImplementedException($"Spreadsheet state {chatState} not implemented.");
    }

    private async Task<Message> ConfigureSpreadsheetAsync(Chat messageChat, string arguments, CancellationToken cancellationToken)
    {
        var spreadsheetId = ExtractSpreadsheetIdFromInput(arguments);

        if (string.IsNullOrWhiteSpace(spreadsheetId) || !SpreadsheetIdRegex().IsMatch(spreadsheetId))
        {
            return await sender.ReplyAsync(messageChat,
                "Planilha inválida.\n" +
                "Verifique o link enviado e tente novamente.",
                $"User tried configuring spreadsheet id with bad arguments: '{arguments}'.",
                logLevel: LogLevel.Information,
                cancellationToken: cancellationToken);
        }

        var validationResponse = await spreadsheetManagerApiClient.ValidateSpreadsheet(spreadsheetId, cancellationToken);

        if (!validationResponse.Success)
        {
            switch (validationResponse.ErrorCode)
            {
                case ErrorCodeEnum.InvalidInput:
                    return await sender.ReplyAsync(messageChat,
                        "O link da planilha é inválido.\n" +
                        "Verifique o link enviado e tente novamente.",
                        "Invalid spreadsheet id.",
                        logLevel: LogLevel.Information,
                        cancellationToken: cancellationToken);
                case ErrorCodeEnum.ResourceNotFound:
                    return await sender.ReplyAsync(messageChat,
                        "Não foi possível encontrar a planilha.\n" +
                        "Verifique o link enviado e tente novamente.",
                        "Spreadsheet not found.",
                        logLevel: LogLevel.Information,
                        cancellationToken: cancellationToken);
                case ErrorCodeEnum.TransactionsSheetNotFound:
                    return await sender.ReplyAsync(messageChat,
                        $"Falha ao configurar a planilha.\n" +
                        $"A planilha enviada não contém a aba de {SpreadsheetConstants.Transactions.SheetName}.",
                        "Spreadsheet not found.",
                        logLevel: LogLevel.Information,
                        cancellationToken: cancellationToken);
                case ErrorCodeEnum.UnauthorizedAccess:
                    return await sender.ReplyAsync(messageChat,
                        "O sistema não tem permissão para acessar planilha enviada.\n" +
                        "Verifique se a planilha está compartilhada corretamente.",
                        logLevel: LogLevel.Information,
                        cancellationToken: cancellationToken);
                case ErrorCodeEnum.UnknownError:
                default:
                    return await sender.ReplyAsync(messageChat,
                        "Ocorreu um erro ao tentar verificar a planilha informada.\n Tente novamente mais tarde.",
                        "Unable to validate spreadsheet with provided ID.",
                        logLevel: LogLevel.Information,
                        cancellationToken: cancellationToken);
            }
        }

        var spreadsheetConfigured = userManagerService.ConfigureSpreadsheet(spreadsheetId, cancellationToken);

        if (!spreadsheetConfigured)
        {
            return await sender.ReplyAsync(messageChat,
                "Sua planilha é válida, mas não foi possível configurar ela no momento. Por favor tente novamente.",
                "SpreadsheetId configuration failed.",
                logLevel: LogLevel.Warning,
                cancellationToken: cancellationToken);
        }

        await chatStateService.ClearState(messageChat.Id);

        return await sender.ReplyAsync(messageChat,
            "Configuração da planilha realizada com sucesso!\n" +
            $"Acione o comando /{StartCommand.StaticCommandName} para ver todas as opções.",
            "SpreadsheetId configuration successful.",
            cancellationToken: cancellationToken);
    }

    private static string ExtractSpreadsheetIdFromInput(string input)
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