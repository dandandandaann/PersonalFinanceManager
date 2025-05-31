using BudgetAutomation.Engine.Interface;

namespace BudgetAutomation.Engine.Handler.Command.Alias;

public class PlanilhaCommandAlias : ICommandAlias
{
    public PlanilhaCommandAlias(IEnumerable<ICommand> commandImplementations) : base(commandImplementations)
    {
        StaticCommandName = "planilha";
        TargetCommandName = SpreadsheetCommand.StaticCommandName;
    }
}