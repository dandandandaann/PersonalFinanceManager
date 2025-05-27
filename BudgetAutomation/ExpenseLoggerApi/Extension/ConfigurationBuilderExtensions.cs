using SharedLibrary.LocalTesting;
using SharedLibrary.Settings;

namespace ExpenseLoggerApi.Extension;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddProjectSpecificConfigurations(
        this IConfigurationBuilder configBuilder, bool localDevelopment = false)
    {
        var devPrefix = LocalDevelopment.Prefix(localDevelopment);

        // Configure AWS Parameter Store
        configBuilder.AddSystemsManager($"/{devPrefix}{BudgetAutomationSettings.Configuration}/");

        return configBuilder;
    }
}