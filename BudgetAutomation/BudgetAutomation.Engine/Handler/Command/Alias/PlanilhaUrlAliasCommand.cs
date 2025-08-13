using BudgetAutomation.Engine.Interface;

namespace BudgetAutomation.Engine.Handler.Command.Alias;
    public class PlanilhaUrlAliasCommand : AliasCommandBase
    {
        public PlanilhaUrlAliasCommand(IEnumerable<ICommand> commandImplementations) : base(commandImplementations)
        {
            TargetCommandName = ReturnSpreadsheetCommand.StaticCommandName;
            base.CommandName = CommandName;
        }

        public static readonly string StaticCommandName = GetCommandNameFromType(typeof(PlanilhaUrlAliasCommand));
        private new string CommandName => StaticCommandName;
    }
