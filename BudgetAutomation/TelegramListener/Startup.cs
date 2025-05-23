using TelegramListener.Extension;

namespace TelegramListener;

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Local development settings
        var isLocalDev = false;//SharedLibrary.LocalTesting.SamStart.IsLocalDev();

        var config = new ConfigurationBuilder().AddProjectSpecificConfigurations(isLocalDev).Build();

        services.AddProjectSpecificServices(config);

    }
}