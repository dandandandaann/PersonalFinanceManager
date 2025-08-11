using BudgetAutomation.Engine.Interface;

namespace BudgetAutomation.Engine.Handler.Command.Alias;

public class CancelarAliasCommand : AliasCommandBase
{
    public CancelarAliasCommand(IEnumerable<ICommand> commandImplementations) : base(commandImplementations)
    {
        TargetCommandName = CancelCommand.StaticCommandName;
        base.CommandName = CommandName;
    }

    public static readonly string StaticCommandName = GetCommandNameFromType(typeof(CancelarAliasCommand));
    private new string CommandName => StaticCommandName;

}