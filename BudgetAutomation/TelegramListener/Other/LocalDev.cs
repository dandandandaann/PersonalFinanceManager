using System.Diagnostics;

namespace TelegramListener.Other;

public static class LocalDev
{
    public static bool IsLocalDev()
    {
        var isLocalDev = (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
            .Equals("Development", StringComparison.OrdinalIgnoreCase);
        return isLocalDev;
    }

    public static void CheckNgrok(bool isDevelopment)
    {
#if DEBUG // Only run this check in Debug configuration
        const string ngrokProcessName = "ngrok";

        if (isDevelopment)
        {
            bool isNgrokRunning = Process.GetProcessesByName(ngrokProcessName).Length > 0;
            if (!isNgrokRunning)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Console.WriteLine("!! WARNING: ngrok process not detected.           !!");
                Console.WriteLine("!! Local testing requiring ngrok might fail.      !!");
                Console.WriteLine("!! Make sure ngrok is running and configured.     !!");
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Console.ResetColor();

                // throw new InvalidOperationException("ngrok is required but not running.");
            }
            else if (isNgrokRunning && isDevelopment)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(">>> ngrok process detected.");
                Console.ResetColor();
            }
        }
#endif
    }
}