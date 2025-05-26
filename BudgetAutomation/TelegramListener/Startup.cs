using TelegramListener.Extension;

namespace TelegramListener;

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Local development settings
        var localDevelopment = false;//SharedLibrary.LocalTesting.SamStart.IsLocalDev();

        var config = new ConfigurationBuilder().AddProjectSpecificConfigurations(localDevelopment).Build();

        services.AddProjectSpecificServices(config);
    }
}