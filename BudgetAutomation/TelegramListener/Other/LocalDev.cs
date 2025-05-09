using System.Diagnostics;

namespace TelegramListener.Other;

public static class LocalDev
{
    public static bool IsLocalDev() => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_LAMBDA_RUNTIME_API"));

    public static void CheckNgrok()
    {
#if DEBUG // Only run this check in Debug configuration
        const string ngrokProcessName = "ngrok";

        if (!IsLocalDev()) return;

        var isNgrokRunning = Process.GetProcessesByName(ngrokProcessName).Length > 0;
        if (isNgrokRunning)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(">>> ngrok process detected.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Console.WriteLine("!! WARNING: ngrok process not detected.           !!");
            Console.WriteLine("!! Local testing requiring ngrok might fail.      !!");
            Console.WriteLine("!! Make sure ngrok is running and configured.     !!");
            Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Console.ResetColor();
        }
#endif
    }
}