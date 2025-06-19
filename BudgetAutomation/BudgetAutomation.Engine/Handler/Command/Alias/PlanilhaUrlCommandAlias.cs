using BudgetAutomation.Engine.Interface;

namespace BudgetAutomation.Engine.Handler.Command.Alias;
    public class PlanilhaUrlCommandAlias : CommandAliasBase
    {
        public PlanilhaUrlCommandAlias(IEnumerable<ICommand> commandImplementations) : base(commandImplementations)
        {
            TargetCommandName = ReturnSpreadsheetCommand.StaticCommandName;
            base.CommandName = CommandName;
        }

        public static readonly string StaticCommandName = GetCommandNameFromType(typeof(PlanilhaUrlCommandAlias));
        private new string CommandName => StaticCommandName;
    }
