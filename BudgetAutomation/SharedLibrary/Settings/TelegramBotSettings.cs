namespace SharedLibrary.Settings;

public class TelegramBotSettings
{
    public const string Configuration = "TelegramBot";

    public string Token { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Handle { get; set; } = string.Empty;
    public string WebhookToken { get; set; } = string.Empty;
    public long StaticId
    {
        set => Id = value;
    }

    public static long Id { get; set; }
}