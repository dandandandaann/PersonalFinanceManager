namespace SharedLibrary.LocalDevelopment;

public static class SamStart
{
    public static bool IsLocalDev() => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_LAMBDA_RUNTIME_API"));


//     /// <summary>
//     /// This method doesn't work when running with 'sam local start-api' because it runs on docker,
//     /// and not on a local process like running it from VisualStudio
//     /// </summary>
//     public static void CheckNgrok()
//     {
//         if (!IsLocalDev()) return;
//
//         Console.WriteLine("Started checking Ngrok");
// // #if DEBUG // Only run this check in Debug configuration
//         const string ngrokProcessName = "ngrok";
//
//         var isNgrokRunning = Process.GetProcessesByName(ngrokProcessName).Length > 0;
//         if (isNgrokRunning)
//         {
//             Console.ForegroundColor = ConsoleColor.Green;
//             Console.WriteLine(">>> ngrok process detected.");
//             Console.ResetColor();
//         }
//         else
//         {
//             Console.ForegroundColor = ConsoleColor.Yellow;
//             Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
//             Console.WriteLine("!! WARNING: ngrok process not detected.           !!");
//             Console.WriteLine("!! Local testing requiring ngrok might fail.      !!");
//             Console.WriteLine("!! Make sure ngrok is running and configured.     !!");
//             Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
//             Console.ResetColor();
//         }
// // #endif
//     }
}