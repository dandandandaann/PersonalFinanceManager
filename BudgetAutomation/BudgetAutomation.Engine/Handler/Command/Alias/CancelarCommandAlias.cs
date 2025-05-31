using BudgetAutomation.Engine.Interface;

namespace BudgetAutomation.Engine.Handler.Command.Alias;

public class CancelarCommandAlias : CommandAliasBase
{
    public CancelarCommandAlias(IEnumerable<ICommand> commandImplementations) : base(commandImplementations)
    {
        TargetCommandName = CancelCommand.StaticCommandName;
        base.CommandName = CommandName;
    }

    public static readonly string StaticCommandName = GetCommandNameFromType(typeof(CancelarCommandAlias));
    private new string CommandName => StaticCommandName;

}