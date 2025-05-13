using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using UserManagerApi.AotTypes;

namespace UserManagerApi;

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup
{
    /// <summary>
    /// Services for Lambda functions can be registered in the services dependency injection container in this method. 
    ///
    /// The services can be injected into the Lambda function through the containing type's constructor or as a
    /// parameter in the Lambda function using the FromService attribute. Services injected for the constructor have
    /// the lifetime of the Lambda compute container. Services injected as parameters are created within the scope
    /// of the function invocation.
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
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
                .WithDynamoDBClient(() => client);
            // contextBuilder = contextBuilder.WithTableNamePrefix("DEV_");
            return contextBuilder.Build();
        });
    }
}