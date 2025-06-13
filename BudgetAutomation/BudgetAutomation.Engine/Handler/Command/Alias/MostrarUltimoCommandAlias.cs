using BudgetAutomation.Engine.Interface;

namespace BudgetAutomation.Engine.Handler.Command.Alias;

public class MostrarUltimoCommandAlias : CommandAliasBase
{
    public MostrarUltimoCommandAlias(IEnumerable<ICommand> commandImplementations) : base(commandImplementations)
    {
        TargetCommandName = LastItemCommand.StaticCommandName;
        base.CommandName = CommandName;
    }

    public static readonly string StaticCommandName = GetCommandNameFromType(typeof(MostrarUltimoCommandAlias));
    private new string CommandName => StaticCommandName;

}