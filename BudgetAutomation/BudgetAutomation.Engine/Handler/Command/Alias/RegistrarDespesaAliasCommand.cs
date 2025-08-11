using BudgetAutomation.Engine.Interface;

namespace BudgetAutomation.Engine.Handler.Command.Alias;

public class RegistrarDespesaAliasCommand : AliasCommandBase
{
    public RegistrarDespesaAliasCommand(IEnumerable<ICommand> commandImplementations) : base(commandImplementations)
    {
        TargetCommandName = LogCommand.StaticCommandName;
        base.CommandName = CommandName;
    }

    public static readonly string StaticCommandName = GetCommandNameFromType(typeof(RegistrarDespesaAliasCommand));
    private new string CommandName => StaticCommandName;
}