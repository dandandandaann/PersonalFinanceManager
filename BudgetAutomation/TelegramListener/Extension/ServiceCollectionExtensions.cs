using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.SQS;
using Microsoft.Extensions.Options;
using SharedLibrary.Settings;
using SharedLibrary.Validator;
using Telegram.Bot;
using TelegramListener.AotTypes;
using TelegramListener.Service;

namespace TelegramListener.Extension;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjectSpecificServices(this IServiceCollection services, IConfiguration config)
    {
        // Serialize Options for AOT
        services.ConfigureTelegramBot<Microsoft.AspNetCore.Http.Json.JsonOptions>(opt => opt.SerializerOptions);
        // #pragma warning disable IL2026
        services.AddAWSLambdaHosting(LambdaEventSource.HttpApi,
            options => { options.Serializer = new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>(); });
        // #pragma warning restore IL2026

        // Configure AWS SQS
        services.AddAWSService<IAmazonSQS>();

        // Bind configurations
        services.Configure<TelegramListenerSettings>(config.GetSection(TelegramListenerSettings.Configuration));
        services.AddSingleton<IValidateOptions<TelegramListenerSettings>, TelegramListenerSettingsValidator>();

        var settingsSection = config.GetSection(TelegramBotSettings.Configuration);
        var settings = settingsSection.Get<TelegramBotSettings>() ??
                       throw new ArgumentNullException(nameof(TelegramBotSettings.Configuration));

        services.Configure<TelegramBotSettings>(settingsSection);
        services.AddSingleton<IValidateOptions<TelegramBotSettings>, TelegramBotSettingsValidator>();

        // Register typed HttpClient directly (optional, but good practice if you need custom HttpClient settings)
        services.AddHttpClient(settings.Handle)
            .AddTypedClient<ITelegramBotClient>(httpClient =>
            {
                TelegramBotClientOptions clientOptions = new(settings.Token);
                return new TelegramBotClient(clientOptions, httpClient);
            });

        // Register services
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<ITelegramUpdateProcessor, TelegramUpdateProcessor>();

        services.AddSingleton<ConfigureWebhook>();

        return services;
    }
}