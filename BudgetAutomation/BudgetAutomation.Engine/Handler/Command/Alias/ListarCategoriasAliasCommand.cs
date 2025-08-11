using BudgetAutomation.Engine.Interface;

namespace BudgetAutomation.Engine.Handler.Command.Alias;

public class ListarCategoriasAliasCommand : AliasCommandBase
{
    public ListarCategoriasAliasCommand(IEnumerable<ICommand> commandImplementations) : base(commandImplementations)
    {
        TargetCommandName = ListCategoriesCommand.StaticCommandName;
        base.CommandName = CommandName;
    }

    public static readonly string StaticCommandName = GetCommandNameFromType(typeof(ListarCategoriasAliasCommand));
    private new string CommandName => StaticCommandName;

}