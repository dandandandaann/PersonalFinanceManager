using SharedLibrary.LocalTesting;
using SharedLibrary.Settings;

namespace BudgetAutomation.Engine.Extension;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddProjectSpecificConfigurations(this IConfigurationBuilder configBuilder, bool localDevelopment = false)
    {
        // Local development settings
        var devPrefix = LocalDevelopment.Prefix(localDevelopment);

        // Configure AWS Parameter Store
        configBuilder.AddSystemsManager($"/{devPrefix}{BudgetAutomationSettings.Configuration}/");

        return configBuilder;
    }
}