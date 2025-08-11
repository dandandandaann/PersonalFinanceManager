using BudgetAutomation.Engine.Interface;

namespace BudgetAutomation.Engine.Handler.Command.Alias;

public class AjudaAliasCommand : AliasCommandBase
{
    public AjudaAliasCommand(IEnumerable<ICommand> commandImplementations) : base(commandImplementations)
    {
        TargetCommandName = HelpCommand.StaticCommandName;
        base.CommandName = CommandName;
    }

    public static readonly string StaticCommandName = GetCommandNameFromType(typeof(AjudaAliasCommand));
    private new string CommandName => StaticCommandName;
}