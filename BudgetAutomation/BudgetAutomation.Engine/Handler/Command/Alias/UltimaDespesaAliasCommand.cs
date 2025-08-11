using BudgetAutomation.Engine.Interface;

namespace BudgetAutomation.Engine.Handler.Command.Alias;

public class UltimaDespesaAliasCommand : AliasCommandBase
{
    public UltimaDespesaAliasCommand(IEnumerable<ICommand> commandImplementations) : base(commandImplementations)
    {
        TargetCommandName = LastItemCommand.StaticCommandName;
        base.CommandName = CommandName;
    }

    public static readonly string StaticCommandName = GetCommandNameFromType(typeof(UltimaDespesaAliasCommand));
    private new string CommandName => StaticCommandName;

}