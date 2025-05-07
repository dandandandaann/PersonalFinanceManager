using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SharedLibrary.Settings;
using SharedLibrary.Validator;
using Telegram.Bot;
using TelegramListener.Other;

namespace TelegramListener;

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup
{
    public async Task ConfigureServices(IServiceCollection services)
    {
        var isLocalDev = LocalDev.IsLocalDev();

        LocalDev.CheckNgrok(isLocalDev);
        var devPrefix = isLocalDev ? "dev-" : "";

        // Serialize Options for AOT
        services.ConfigureTelegramBot<Microsoft.AspNetCore.Http.Json.JsonOptions>(opt => opt.SerializerOptions);

        var configBuilder = new ConfigurationBuilder();

        // Configure AWS Parameter Store
        configBuilder.AddSystemsManager($"/{devPrefix}{BudgetAutomationSettings.Configuration}/");
        var config = configBuilder.Build();

        // Bind Bot configuration
        services.Configure<BotSettings>(config.GetSection(BotSettings.Configuration));
        services.AddSingleton<IValidateOptions<BotSettings>, BotSettingsValidator>();

        // Register typed HttpClient directly (optional, but good practice if you need custom HttpClient settings)
        services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                var botConfig = sp.GetRequiredService<IOptions<BotSettings>>().Value;
                TelegramBotClientOptions options = new(botConfig.Token);
                return new TelegramBotClient(options, httpClient);
            });


        services.AddHostedService<ConfigureWebhook>();

    }
}