using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Serialization.SystemTextJson;
using SharedLibrary.LocalTesting;
using UserManagerApi.AotTypes;
using UserManagerApi.Service;

namespace UserManagerApi.Extension;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjectSpecificServices(
        this IServiceCollection services, IConfiguration config, bool localDevelopment = false)
    {
        // #pragma warning disable IL2026
        services.AddAWSLambdaHosting(LambdaEventSource.HttpApi,
            options => { options.Serializer = new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>(); });
        // #pragma warning restore IL2026

        services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(RegionEndpoint.USEast2));

        services.AddScoped<IDynamoDBContext>(sp =>
        {
            var client = sp.GetRequiredService<IAmazonDynamoDB>();
            var contextBuilder = new DynamoDBContextBuilder()
                .WithDynamoDBClient(() => client)
                .ConfigureContext(dynamoDbContextConfig =>
                {
                    dynamoDbContextConfig.TableNamePrefix = LocalDevelopment.Prefix(localDevelopment);
                });
            return contextBuilder.Build();
        });

        // Register services
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}