using BudgetAutomation.Engine.Interface;

namespace BudgetAutomation.Engine.Handler.Command.Alias;

public class PlanilhaCommandAlias : CommandAliasBase
{
    public PlanilhaCommandAlias(IEnumerable<ICommand> commandImplementations) : base(commandImplementations)
    {
        TargetCommandName = SpreadsheetCommand.StaticCommandName;
        base.CommandName = CommandName;
    }

    public static readonly string StaticCommandName = GetCommandNameFromType(typeof(PlanilhaCommandAlias));
    private new string CommandName => StaticCommandName;

}