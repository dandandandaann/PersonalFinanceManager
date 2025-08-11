using BudgetAutomation.Engine.Interface;

namespace BudgetAutomation.Engine.Handler.Command.Alias;

public class PlanilhaConfigurarAliasCommand : AliasCommandBase
{
    public PlanilhaConfigurarAliasCommand(IEnumerable<ICommand> commandImplementations) : base(commandImplementations)
    {
        TargetCommandName = SpreadsheetCommand.StaticCommandName;
        base.CommandName = CommandName;
    }

    public static readonly string StaticCommandName = GetCommandNameFromType(typeof(PlanilhaConfigurarAliasCommand));
    private new string CommandName => StaticCommandName;

}