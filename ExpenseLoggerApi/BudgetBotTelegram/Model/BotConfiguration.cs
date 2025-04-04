namespace BudgetBotTelegram.Model;

public class BotConfiguration
{
    public const string Configuration = "BotConfiguration";

    public string Token { get; set; } = string.Empty;
    public string HostAddress { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Handle { get; set; } = string.Empty;
}