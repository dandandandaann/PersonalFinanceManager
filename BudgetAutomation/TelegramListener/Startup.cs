using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TelegramListener.Extension;

namespace TelegramListener;

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Local development settings
        var isLocalDev = SharedLibrary.LocalDevelopment.SamStart.IsLocalDev();

        var config = new ConfigurationBuilder().AddProjectSpecificConfigurations(isLocalDev).Build();

        services.AddProjectSpecificServices(config);

    }
}