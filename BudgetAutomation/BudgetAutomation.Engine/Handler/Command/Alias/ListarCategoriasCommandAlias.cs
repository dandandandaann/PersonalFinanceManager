using BudgetAutomation.Engine.Interface;

namespace BudgetAutomation.Engine.Handler.Command.Alias;

public class ListarCategoriasCommandAlias : CommandAliasBase
{
    public ListarCategoriasCommandAlias(IEnumerable<ICommand> commandImplementations) : base(commandImplementations)
    {
        TargetCommandName = ListCategoriesCommand.StaticCommandName;
        base.CommandName = CommandName;
    }

    public static readonly string StaticCommandName = GetCommandNameFromType(typeof(ListarCategoriasCommandAlias));
    private new string CommandName => StaticCommandName;

}