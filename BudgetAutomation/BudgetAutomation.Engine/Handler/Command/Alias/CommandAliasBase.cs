using System.Text.RegularExpressions;
using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Model;
using SharedLibrary.Telegram;
using Telegram.Bot.Types.Enums;
using MessageEntity = Telegram.Bot.Types.MessageEntity;
using MessageEntityType = SharedLibrary.Telegram.Enums.MessageEntityType;

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

        message.Entities =
        [
            new SharedLibrary.Telegram.MessageEntity
            {
                Offset = 0,
                Length = TargetCommandName.Length + 1,
                Type = MessageEntityType.BotCommand
            }
        ];

        var pattern = new Regex($@"^(?:/?){CommandName}\b");
        message.Text = pattern.Replace(message.Text, $"/{TargetCommandName}");

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