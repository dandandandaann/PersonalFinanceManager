using System.Diagnostics;

namespace BudgetBotTelegram;

public static class LocalDev
{
    public static void CheckNgrok(WebApplicationBuilder webApplicationBuilder)
    {
#if DEBUG // Only run this check in Debug configuration
        const string ngrokProcessName = "ngrok";

        if (webApplicationBuilder.Environment.IsDevelopment())
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
            else if (isNgrokRunning && webApplicationBuilder.Environment.IsDevelopment())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(">>> ngrok process detected.");
                Console.ResetColor();
            }
        }
#endif
    }
}