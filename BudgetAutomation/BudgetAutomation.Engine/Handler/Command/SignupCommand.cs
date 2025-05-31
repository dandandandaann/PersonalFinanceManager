using System.Text;
using System.Text.RegularExpressions;
using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Misc;
using BudgetAutomation.Engine.Model;
using BudgetAutomation.Engine.Service;
using SharedLibrary.Telegram;
using SharedLibrary.Telegram.Enums;

namespace BudgetAutomation.Engine.Handler.Command;

public partial class SignupCommand(
    ISenderGateway sender,
    IUserApiClient userApiClient) : ICommand
{
    public string CommandName => "signup";

    public async Task<Message> HandleAsync(Message message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.From);
        ArgumentNullException.ThrowIfNull(message.Text);

        var telegramId = message.From.Id;
        var username = message.From.Username;

        // Send an initial reply indicating the process has started
        var replyAttempting = sender.ReplyAsync(
            message.Chat,
            "Tentando fazer seu cadastro...", "Processo de cadastro iniciado.",
            cancellationToken: cancellationToken);

        if (UserManagerService.UserSignedIn)
        {
            await replyAttempting;
            return await sender.ReplyAsync(message.Chat,
                "O cadastro falhou. Você já está conectado.",
                "User signup failed (already signed in).",
                logLevel: LogLevel.Warning,
                cancellationToken: cancellationToken);
        }

        try
        {
            if (
                !Utility.TryExtractCommandArguments(message.Text, CommandName, EmailRegex, out var signupArguments) ||
                string.IsNullOrWhiteSpace(signupArguments)
                )
            {
                return await sender.ReplyAsync(message.Chat,
                    "Por favor inclua seu e-mail para o cadastro.",
                    $"User tried signing up with invalid arguments: '{signupArguments}'.",
                    logLevel: LogLevel.Information,
                    cancellationToken: cancellationToken);
            }

            var response = await userApiClient.SignupUserAsync(
                telegramId, username: username, email: signupArguments, cancellationToken: cancellationToken);

            if (!response.Success)
            {
                // UserApiClient returns Success=false if user already exists or on API error
                // TODO: Differentiate between 'already exists' and 'other error' in UserApiClient response
                return await sender.ReplyAsync(message.Chat,
                    "Falha no cadastro. Tente novamente mais tarde.",
                    "User signup failed (already exists or API error).",
                    logLevel: LogLevel.Warning,
                    cancellationToken: cancellationToken);
            }

            var welcomeMessage = new StringBuilder();
            welcomeMessage.AppendLine("Cadastro realizado com sucesso.");
            welcomeMessage.AppendLine();
            welcomeMessage.Append("<b>");
            welcomeMessage.Append(response.User?.Username == null ? "Bem vindo(a)!" : $"Bem vindo(a), {response.User.Username}!");
            welcomeMessage.AppendLine("</b>");
            welcomeMessage.AppendLine($"Por favor digite /{StartCommand.StaticCommandName} para ver os comandos disponíveis " +
                                      $"e /{SpreadsheetCommand.StaticCommandName} para configurar sua planilha.");

            return await sender.ReplyAsync(message.Chat,
                welcomeMessage.ToString(),
                "Cadastro do usuário realizado com sucesso.",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            if (ex is InvalidUserInputException or UnauthorizedAccessException)
                throw;

            // Catch exceptions from the API client (e.g., network issues, deserialization errors)
            return await sender.ReplyAsync(message.Chat,
                "Ocorreu um erro durante o cadastro. Por favor, tente novamente mais tarde.",
                $"Exception during signup: {ex.Message}",
                logLevel: LogLevel.Error,
                cancellationToken: cancellationToken);
        }
    }

    public Task<Message> HandleAsync(Message message, ChatState chatState, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
    private static partial Regex EmailRegex();
}