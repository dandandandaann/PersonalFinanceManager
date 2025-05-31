using System.Text.RegularExpressions;
using BudgetAutomation.Engine.Model;
using SharedLibrary.Telegram;

namespace BudgetAutomation.Engine.Interface;

public abstract class ICommandAlias(IEnumerable<ICommand> commandImplementations) : ICommand
{
    public required string TargetCommandName { get; init; }
    public string CommandName => StaticCommandName;

    public static string StaticCommandName;

    public async Task<Message> HandleAsync(Message message, CancellationToken cancellationToken)
    {
        var targetCommand = commandImplementations.First(x => x.CommandName == TargetCommandName);

        return await targetCommand.HandleAsync(ConvertAliasToCommand(message), cancellationToken);
    }

    public async Task<Message> HandleAsync(Message message, ChatState chatState, CancellationToken cancellationToken)
    {
        var targetCommand = commandImplementations.First(x => x.CommandName == TargetCommandName);

        return await targetCommand.HandleAsync(ConvertAliasToCommand(message), chatState, cancellationToken);
    }

    private Message ConvertAliasToCommand(Message message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message.Text);

        message.Entities = null;

        var pattern = new Regex($@"^(?:/?){CommandName}\b");
        message.Text = pattern.Replace(message.Text, TargetCommandName);

        return message;
    }
}