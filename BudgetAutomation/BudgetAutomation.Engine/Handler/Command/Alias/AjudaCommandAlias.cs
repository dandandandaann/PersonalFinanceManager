using BudgetAutomation.Engine.Interface;

namespace BudgetAutomation.Engine.Handler.Command.Alias;

public class AjudaCommandAlias : CommandAliasBase
{
    public AjudaCommandAlias(IEnumerable<ICommand> commandImplementations) : base(commandImplementations)
    {
        TargetCommandName = LogCommand.StaticCommandName;
        base.CommandName = CommandName;
    }

    public static readonly string StaticCommandName = GetCommandNameFromType(typeof(AjudaCommandAlias));
    private new string CommandName => StaticCommandName;
}