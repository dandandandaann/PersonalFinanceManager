using BudgetAutomation.Engine.Extension;

namespace BudgetAutomation.Engine;

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var configBuilder = new ConfigurationBuilder();

        var localDevelopment = false;//SharedLibrary.LocalTesting.SamStart.IsLocalDev();

        var config = configBuilder.AddProjectSpecificConfigurations(localDevelopment).Build();

        services.AddProjectSpecificServices(config);
    }
}