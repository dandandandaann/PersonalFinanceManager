namespace SharedLibrary.Settings;

public class BotSettings
{
    public const string Configuration = "BotConfiguration";

    public string Token { get; set; } = string.Empty;
    public string HostAddress { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Handle { get; set; } = string.Empty;
    public string WebhookToken { get; set; } = string.Empty;
    public long StaticId
    {
        set => Id = value;
    }

    public static long Id { get; set; } = 0;
}