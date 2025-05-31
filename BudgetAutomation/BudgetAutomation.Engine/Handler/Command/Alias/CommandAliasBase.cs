using System.Text.RegularExpressions;
using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Model;
using SharedLibrary.Telegram;

namespace BudgetAutomation.Engine.Handler.Command.Alias;

public abstract class CommandAliasBase(IEnumerable<ICommand> commandImplementations) : ICommand
{
    public required string TargetCommandName { get; init; }
    public string CommandName { get; protected init; } = null!;

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

    protected static string GetCommandNameFromType(Type type)
    {
        var classSuffix = "CommandAlias";
        string name = type.Name;
        if (name.EndsWith(classSuffix, StringComparison.OrdinalIgnoreCase))
        {
            name = name.Substring(0, name.Length - classSuffix.Length);
        }
        return name.ToLowerInvariant();
    }
}