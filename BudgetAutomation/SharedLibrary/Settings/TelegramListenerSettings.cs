namespace SharedLibrary.Settings;

public class TelegramListenerSettings
{
    public const string Configuration = "TelegramListener";

    public string TelegramUpdateQueue { get; set; } = string.Empty;
}