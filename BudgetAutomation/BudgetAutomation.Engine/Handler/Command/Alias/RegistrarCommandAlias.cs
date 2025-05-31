using BudgetAutomation.Engine.Interface;

namespace BudgetAutomation.Engine.Handler.Command.Alias;

public class RegistrarCommandAlias : ICommandAlias
{
    public RegistrarCommandAlias(IEnumerable<ICommand> commandImplementations) : base(commandImplementations)
    {
        StaticCommandName = "registrar";
        TargetCommandName = LogCommand.StaticCommandName;
    }
}