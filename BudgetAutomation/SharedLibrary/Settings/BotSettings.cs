namespace SharedLibrary.Settings;

public class BotSettings
{
    public const string Configuration = "BotConfiguration";

    public string Token { get; set; } = string.Empty;
    public string StaticId
    {
        set => Id = long.Parse(value);
    }

    public static long Id { get; set; }
}