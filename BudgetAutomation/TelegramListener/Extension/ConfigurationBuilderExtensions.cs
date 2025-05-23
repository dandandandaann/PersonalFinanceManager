using SharedLibrary.Settings;

namespace TelegramListener.Extension;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddProjectSpecificConfigurations(this IConfigurationBuilder configBuilder, bool localDevelopment = false)
    {
        var devPrefix = localDevelopment ? "dev-" : "";

        // Configure AWS Parameter Store
        configBuilder.AddSystemsManager($"/{devPrefix}{BudgetAutomationSettings.Configuration}/");

        if (localDevelopment)
        {
            configBuilder.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
            Console.WriteLine("Start with local development settings.");
        }
        return configBuilder;
    }
}