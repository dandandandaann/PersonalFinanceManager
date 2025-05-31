using BudgetAutomation.Engine.Interface;

namespace BudgetAutomation.Engine.Handler.Command.Alias;

public class CadastrarCommandAlias : CommandAliasBase
{
    public CadastrarCommandAlias(IEnumerable<ICommand> commandImplementations) : base(commandImplementations)
    {
        TargetCommandName = SignupCommand.StaticCommandName;
        base.CommandName = CommandName;
    }

    public static readonly string StaticCommandName = GetCommandNameFromType(typeof(CadastrarCommandAlias));
    private new string CommandName => StaticCommandName;

}