using BudgetAutomation.Engine.Interface;

namespace BudgetAutomation.Engine.Handler.Command.Alias;

public class RegistrarCommandAlias : CommandAliasBase
{
    public RegistrarCommandAlias(IEnumerable<ICommand> commandImplementations) : base(commandImplementations)
    {
        TargetCommandName = LogCommand.StaticCommandName;
        base.CommandName = CommandName;
    }

    public static readonly string StaticCommandName = GetCommandNameFromType(typeof(RegistrarCommandAlias));
    private new string CommandName => StaticCommandName;
}