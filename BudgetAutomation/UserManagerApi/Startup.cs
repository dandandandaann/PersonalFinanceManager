using UserManagerApi.Extension;

namespace UserManagerApi;

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddProjectSpecificServices(new ConfigurationManager(), true);
    }
}