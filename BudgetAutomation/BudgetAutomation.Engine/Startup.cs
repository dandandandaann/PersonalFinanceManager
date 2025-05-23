using BudgetAutomation.Engine.Extension;

namespace BudgetAutomation.Engine;

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var configBuilder = new ConfigurationBuilder();

        // Local development settings
        var localDevelopment = SharedLibrary.LocalDevelopment.SamStart.IsLocalDev();

        var config = configBuilder.AddProjectSpecificConfigurations(localDevelopment).Build();

        services.AddProjectSpecificServices(config);
    }
}