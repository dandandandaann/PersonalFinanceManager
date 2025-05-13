namespace SharedLibrary.Settings;

public class TelegramListenerSettings
{
    public const string Configuration = "TelegramListener";

    public string HostAddress { get; set; } = string.Empty;
    public string TelegramUpdateQueue { get; set; } = string.Empty;
}