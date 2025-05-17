using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.SQS;
using BudgetBotTelegram.ApiClient;
using BudgetBotTelegram.Handler;
using BudgetBotTelegram.Handler.Command;
using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using SharedLibrary.Settings;
using SharedLibrary.Validator;
using Telegram.Bot;

namespace BudgetBotTelegram;

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var configBuilder = new ConfigurationBuilder();

        // Local development settings
        var isLocalDev = SharedLibrary.LocalDevelopment.SamStart.IsLocalDev();
        var devPrefix = isLocalDev ? "dev-" : "";

        // Configure AWS Parameter Store
        configBuilder.AddSystemsManager($"/{devPrefix}{BudgetAutomationSettings.Configuration}/");

        if (isLocalDev)
        {
            configBuilder.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
        }

        var config = configBuilder.Build();

        // Configure AWS Services
        services.AddAWSService<IAmazonSQS>();
        services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(RegionEndpoint.USEast2));
        services.AddScoped<IDynamoDBContext>(sp =>
        {
            var client = sp.GetRequiredService<IAmazonDynamoDB>();
            var contextBuilder = new DynamoDBContextBuilder()
                .WithDynamoDBClient(() => client);
            // contextBuilder = contextBuilder.WithTableNamePrefix("DEV_");
            return contextBuilder.Build();
        });

        // Bind configurations
        var settingsSection = config.GetSection(TelegramBotSettings.Configuration);
        var settings = settingsSection.Get<TelegramBotSettings>() ??
                       throw new ArgumentNullException(nameof(TelegramBotSettings.Configuration));

        services.Configure<TelegramBotSettings>(settingsSection);
        services.AddSingleton<IValidateOptions<TelegramBotSettings>, TelegramBotSettingsValidator>();

        services.Configure<ExpenseLoggerApiClientSettings>(config.GetSection(ExpenseLoggerApiClientSettings.Configuration));
        services.AddSingleton<IValidateOptions<ExpenseLoggerApiClientSettings>, ExpenseLoggerApiClientSettingsValidator>();

        services.Configure<UserApiClientSettings>(config.GetSection(UserApiClientSettings.Configuration));
        services.AddSingleton<IValidateOptions<UserApiClientSettings>, UserApiClientSettingsValidator>();


        // Register typed HttpClient directly (optional, but good practice if you need custom HttpClient settings)
        services.AddHttpClient(settings.Handle)
            .AddTypedClient<ITelegramBotClient>(httpClient =>
            {
                TelegramBotClientOptions clientOptions = new(settings.Token);
                return new TelegramBotClient(clientOptions, httpClient);
            });

        // Register services
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddHttpClient<IExpenseLoggerApiClient, ExpenseLoggerApiClient>();
        services.AddHttpClient<IUserApiClient, UserApiClient>();
        services.AddTransient<SqsUpdateProcessor>();
        services.AddScoped<IChatStateService, ChatStateService>();
        services.AddScoped<IUserManagerService, UserManagerService>();
        services.AddSingleton<ISenderGateway, SenderGateway>();

        // Register handlers
        services.AddScoped<IUpdateHandler, UpdateHandler>();
        services.AddScoped<IMessageHandler, MessageHandler>();
        services.AddScoped<ITextMessageHandler, TextMessageHandler>();
        services.AddScoped<ICommandHandler, CommandHandler>();

        // Register commands
        services.AddScoped<ILogCommand, LogCommand>();
        services.AddScoped<ICancelCommand, CancelCommand>();
        services.AddScoped<ISignupCommand, SignupCommand>();
    }
}